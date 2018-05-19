using Newtonsoft.Json;

namespace PancakeTeam.Rollbar
{
    internal class RollbarResponse
	{
		[JsonProperty("err")]
		public int Error { get; set; }

		public RollbarResult Result { get; set; }
	}
}