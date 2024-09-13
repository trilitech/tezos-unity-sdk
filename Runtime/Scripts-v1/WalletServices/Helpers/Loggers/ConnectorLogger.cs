using System;
using Microsoft.Extensions.Logging;
using TezosSDK.Helpers.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TezosSDK.WalletServices.Helpers.Loggers
{

	public class ConnectorLogger : ILogger
	{
		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			var message = formatter(state, exception);

			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			if (exception != null)
			{
				message += "\nException: " + exception;
			}

			TezosLogger.LogError("BEACON MESSAGE: " + message);
		}
	}

}