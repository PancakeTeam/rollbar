using Newtonsoft.Json;

namespace PancakeTeam.Rollbar
{
	[JsonConverter(typeof(ErrorLevelConverter))]
	public enum LogLevel
	{
		Critical,
		Error,
		Warning,
		Info,
		Debug
	}
}