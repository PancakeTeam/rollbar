using System.Linq;
using Newtonsoft.Json.Linq;

namespace PancakeTeam.Rollbar
{
    internal static class JsonScrubber
    {
        private const string defaultScrubMask = "***";
        private static readonly string[] scrubbedFields = { "Authorization", "password", "newPassword" };

        public static string ScrubJson(string jsonData)
        {
            JObject json = JObject.Parse(jsonData);

            ScrubJson(json);

            return json.ToString();
        }

        private static void ScrubJson(JToken json)
        {
            if (json is JProperty property)
            {
                ScrubJson(property);
                return;
            }

            foreach (var child in json.Children())
            {
                ScrubJson(child);
            }
        }

        public static void ScrubJson(JProperty json)
        {
            if (scrubbedFields.Contains(json.Name))
            {
                json.Value = defaultScrubMask;
                return;
            }

            if (!(json.Value is JContainer propertyValue)) return;
            foreach (var child in propertyValue)
            {
                ScrubJson(child);
            }
        }

    }
}