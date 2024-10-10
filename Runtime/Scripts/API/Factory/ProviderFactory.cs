using System.Collections.Generic;
using Tezos.Provider;

namespace Tezos.API
{
	public static class ProviderFactory
	{
		private static List<IProviderController> _operationProviders;
		
		public static void Init(List<IProviderController> operationProviders) => _operationProviders = operationProviders;

		public static IProviderController GetConnectedProviderController()
		{
			var provider = _operationProviders.Find(op => op.IsConnected);
			if(provider == default)
				throw new ConnectionRequiredException("No connection found");

			return provider;
		}

		public static IProviderController GetProviderController(ProviderType providerType)
		{
			var provider = _operationProviders.Find(op => op.ProviderType == providerType);
			if(provider == default)
				throw new ConnectionRequiredException($"No provider found with type:{providerType}");

			return provider;
		}
	}
}