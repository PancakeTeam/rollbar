using Newtonsoft.Json;

namespace PancakeTeam.Rollbar
{
    internal class CodeContext
	{
		[JsonProperty("pre", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string[] Pre { get; set; }

		[JsonProperty("post", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string[] Post { get; set; }
	}
}
