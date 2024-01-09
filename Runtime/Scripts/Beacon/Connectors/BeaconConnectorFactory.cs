using System;
using TezosSDK.Tezos;
using UnityEngine;

namespace TezosSDK.Beacon
{

	public static class BeaconConnectorFactory
	{
		public static IBeaconConnector CreateConnector(RuntimePlatform platform, WalletEventManager eventManager)
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
					return new BeaconConnectorDotNet(eventManager);
				default:
					throw new ArgumentException("Unsupported platform");
			}
		}
	}

}