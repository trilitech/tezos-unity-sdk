using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TezosSDK.WalletServices.Helpers.Loggers
{

	public class ConnectorLoggerProvider : ILoggerProvider
	{
		public void Dispose()
		{
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new ConnectorLogger();
		}
	}

}