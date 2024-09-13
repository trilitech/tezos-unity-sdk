using System;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.WalletServices.Connectors.DotNet;
using TezosSDK.WalletServices.Connectors.Kukai;
using TezosSDK.WalletServices.Connectors.WebGL;

namespace TezosSDK.WalletServices.Connectors
{

	public static class WalletConnectorFactory
	{
		private static BeaconConnectorWebGl  _BEACON_CONNECTOR_WEB_GL;
		private static BeaconConnectorDotNet _BEACON_CONNECTOR_DOT_NET;
		private static KukaiConnector        _KUKAI_CONNECTOR;

		static WalletConnectorFactory()
		{
			_BEACON_CONNECTOR_WEB_GL  = new BeaconConnectorWebGl();
			_BEACON_CONNECTOR_DOT_NET = new BeaconConnectorDotNet();
			_KUKAI_CONNECTOR          = new KukaiConnector();
		}
		
		public static IWalletConnector GetConnector(ConnectorType connectorType)
		{
			return connectorType switch
				   {
					   ConnectorType.BeaconWebGl  => _BEACON_CONNECTOR_WEB_GL,
					   ConnectorType.BeaconDotNet => _BEACON_CONNECTOR_DOT_NET,
					   ConnectorType.Kukai        => _KUKAI_CONNECTOR,
					   _                          => throw new ArgumentException("Unknown connector type")
				   };
		}
	}

}