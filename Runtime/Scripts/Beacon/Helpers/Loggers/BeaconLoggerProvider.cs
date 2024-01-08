using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TezosSDK.Beacon.Loggers
{

	public class BeaconLoggerProvider : ILoggerProvider
	{
		public void Dispose()
		{
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new BeaconLogger();
		}
	}

}