#region

using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#endregion

namespace TezosSDK.Beacon
{

	public class MyLoggerProvider : ILoggerProvider
	{
		#region IDisposable Implementation

		public void Dispose()
		{
		}

		#endregion

		#region ILoggerProvider Implementation

		public ILogger CreateLogger(string categoryName)
		{
			return new MyLogger();
		}

		#endregion

		#region Nested Types

		public class MyLogger : ILogger
		{
			#region ILogger Implementation

			public IDisposable BeginScope<TState>(TState state)
			{
				return null;
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return true;
			}

			public void Log<TState>(
				LogLevel logLevel,
				EventId eventId,
				TState state,
				Exception exception,
				Func<TState, Exception, string> formatter)
			{
				if (exception != null)
				{
					Debug.LogException(exception);
				}

				//Debug.Log(state.ToString());
			}

			#endregion
		}

		#endregion
	}

}