using System;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Connectors.DotNet;
using TezosSDK.WalletServices.Connectors.WebGL;
using UnityEngine;

namespace TezosSDK.WalletServices.Connectors
{

	public static class WalletConnectorFactory
	{
		public static IWalletConnector CreateConnector(RuntimePlatform platform, WalletEventManager eventManager)
		{
			switch (platform)
			{
				case RuntimePlatform.WebGLPlayer:
					return new BeaconConnectorWebGl(eventManager);
				case RuntimePlatform.IPhonePlayer:
				case RuntimePlatform.Android:
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.LinuxPlayer:
				case RuntimePlatform.LinuxEditor:
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.OSXEditor:
					return new BeaconConnectorDotNet(eventManager);
				default:
					throw new ArgumentException("Unsupported platform");
			}
		}
	}

}