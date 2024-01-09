using UnityEngine;

namespace TezosSDK.Helpers
{

	public static class Logger
	{
		public enum LogLevel
		{
			None,
			Error,
			Warning,
			Info,
			Debug
		}

		public static LogLevel CurrentLogLevel = LogLevel.Debug;

		public static void Log(string message, LogLevel logLevel = LogLevel.Info)
		{
			if (logLevel > CurrentLogLevel)
			{
				return;
			}

			switch (logLevel)
			{
				case LogLevel.Debug:
					Debug.Log(message);
					break;
				case LogLevel.Info:
					Debug.Log(message);
					break;
				case LogLevel.Warning:
					Debug.LogWarning(message);
					break;
				case LogLevel.Error:
					Debug.LogError(message);
					break;
			}
		}

		public static void LogDebug(string message)
		{
			Log(message, LogLevel.Debug);
		}

		public static void LogInfo(string message)
		{
			Log(message);
		}

		public static void LogWarning(string message)
		{
			Log(message, LogLevel.Warning);
		}

		public static void LogError(string message)
		{
			Log(message, LogLevel.Error);
		}
	}

}