using Newtonsoft.Json;

namespace PancakeTeam.Rollbar
{
	public struct Person
	{
		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("username", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string UserName { get; set; }

		[JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Email { get; set; }
	}
}
