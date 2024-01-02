using System;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;
using UnityEngine;

namespace TezosSDK.Beacon
{

	public static class BeaconConnectorFactory
	{
		public static IBeaconConnector CreateConnector(RuntimePlatform platform, WalletEventManager eventManager, DAppMetadata dAppMetadata)
		{
			switch (platform)
			{
				case RuntimePlatform.WebGLPlayer:
					return new BeaconConnectorWebGl();
				case RuntimePlatform.IPhonePlayer:
				case RuntimePlatform.Android:
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.LinuxPlayer:
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.OSXEditor:
					return new BeaconConnectorDotNet(eventManager, TezosConfig.Instance.Network.ToString(),
						TezosConfig.Instance.RpcBaseUrl, dAppMetadata);
				default:
					throw new ArgumentException("Unsupported platform");
			}
		}
	}

}