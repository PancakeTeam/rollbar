using System;
using Newtonsoft.Json;

namespace PancakeTeam.Rollbar
{
    internal class Payload
	{
		public Payload(string accessToken, Data data)
		{
			if (string.IsNullOrWhiteSpace(accessToken))
			{
				throw new ArgumentNullException(nameof(accessToken));
			}

		    AccessToken = accessToken;
			Data = data ?? throw new ArgumentNullException(nameof(data));
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		[JsonProperty("access_token", Required = Required.Always)]
		public string AccessToken { get; private set; }

		[JsonProperty("data", Required = Required.Always)]
		public Data Data { get; private set; }
	}
}
