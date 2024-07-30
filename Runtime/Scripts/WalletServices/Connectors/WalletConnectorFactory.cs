using System;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Connectors.DotNet;
using TezosSDK.WalletServices.Connectors.Kukai;
using TezosSDK.WalletServices.Connectors.WebGL;

namespace TezosSDK.WalletServices.Connectors
{

	public static class WalletConnectorFactory
	{
		public static IWalletConnector CreateConnector(ConnectorType connectorType, WalletEventManager eventManager)
		{
			switch (connectorType)
			{
				case ConnectorType.BeaconWebGl:
					return new BeaconConnectorWebGl(eventManager);
				case ConnectorType.BeaconDotNet:
					return new BeaconConnectorDotNet(eventManager);
				case ConnectorType.Kukai:
					return new KukaiConnector(eventManager);
				default:
					throw new ArgumentException("Unknown connector type");
			}
		}
	}

}