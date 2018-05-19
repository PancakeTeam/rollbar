using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PancakeTeam.Rollbar
{
    public class RollbarLogger
    {
        private readonly string _accessToken;
        private readonly string _environment;
        private readonly HttpClient _client;

        public Server Server { get; set; }

        public RollbarLogger(string accessToken, string environment)
        {
            this._accessToken = accessToken;
            this._environment = environment;
            this._client = new HttpClient { BaseAddress = new Uri("https://api.rollbar.com/api/1/") };
        }

        public IDisposable BeginScope<TState>(TState state) => new NothingDisposer();

        private class NothingDisposer : IDisposable { public void Dispose() { } }

        public bool IsEnabled(LogLevel logLevel) => true;

        public Task Log(LogLevel logLevel, string message, IDictionary<string, object> customData, System.Exception exception, IHttpContextAccessor httpContextAccessor, Person? personData = null)
        {
            if (!IsEnabled(logLevel)) return Task.CompletedTask;

            var body = BuildBody(message, exception, customData);

            return SendBody(body, Translate(logLevel), customData, httpContextAccessor, personData);
        }

        private Body BuildBody(string message, System.Exception exception, IDictionary<string, object> structuredData)
        {
            if (exception != null)
            {
                if (!String.IsNullOrWhiteSpace(message))
                    structuredData.Add("message", message);

                if (exception is AggregateException ae)
                    return new Body(ae);

                return new Body(exception);
            }

            return new Body(new Message(message));
        }

        private Task SendBody(Body body, ErrorLevel level, IDictionary<string, object> custom, IHttpContextAccessor httpContextAccessor, Person? personData)
        {
            var payload = new Payload(_accessToken, new Data(_environment, body)
            {
                GuidUuid = Guid.NewGuid(),
                Custom = custom,
                Level = level,
                Person = personData,
                Server = Server,
                Request = BuildRequest(httpContextAccessor)
            });

            string json = JsonConvert.SerializeObject(payload);
            var scrubbedJson = JsonScrubber.ScrubJson(json);

            return _client.PostAsync("item/", new StringContent(scrubbedJson, Encoding.UTF8, "application/json"));
        }

        private Request BuildRequest(IHttpContextAccessor httpContextAccessor)
        {
            var ctx = httpContextAccessor?.HttpContext;
            var req = ctx?.Request;
            if (req == null) return null;

            var result = new Request
            {
                Url = $"{req.Scheme}://{req.Host}{req.PathBase}{req.Path}",
                QueryString = req.QueryString.Value,
                Method = req.Method.ToUpper(),
                Headers = req.Headers.Aggregate(new Dictionary<string, string>(), (dic, item) =>
                {
                    dic.Add(item.Key, item.Value);
                    return dic;
                }),
                UserIp = ctx.Connection?.RemoteIpAddress?.ToString(),
                PostBody = ReadPostBody(req.Body)
            };

            if (req.Query.Any())
            {
                result.GetParams = req.Query.Aggregate(new Dictionary<string, object>(), (dic, item) =>
                {
                    dic.Add(item.Key, (string)item.Value);
                    return dic;
                });
            }

            if (req.HasFormContentType && req.Form != null)
            {
                result.PostParams = req.Form.Aggregate(new Dictionary<string, object>(), (dic, item) =>
                {
                    dic.Add(item.Key, (string)item.Value);
                    return dic;
                });
            }

            return result;
        }

        private string ReadPostBody(Stream reqStream)
        {
            if (reqStream == null || reqStream == Stream.Null)
                return null;

            if (!reqStream.CanSeek)
                // stream isn't rewindable; don't fuck with it
                // need to call app.UseRollbarRequestLogger to get POST bodies
                return null;

            if (reqStream.Position != 0)
                reqStream.Position = 0;

            var reader = new StreamReader(reqStream);
            return reader.ReadToEnd();
        }

        private static ErrorLevel Translate(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return ErrorLevel.Debug;
                case LogLevel.Info: return ErrorLevel.Info;
                case LogLevel.Warning: return ErrorLevel.Warning;
                case LogLevel.Error: return ErrorLevel.Error;
                case LogLevel.Critical: return ErrorLevel.Critical;
                default: return ErrorLevel.Debug;
            }
        }
    }
}