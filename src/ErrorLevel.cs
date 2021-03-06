﻿using Newtonsoft.Json;

namespace PancakeTeam.Rollbar
{
	[JsonConverter(typeof(ErrorLevelConverter))]
	internal enum ErrorLevel
	{
		Critical,
		Error,
		Warning,
		Info,
		Debug
	}
}