#if UNITY_ANDROID
using UnityEngine;

namespace BeaconSDK
{
	/// <summary>
	/// Android implementation of the BeaconConnector.
	/// Binds the functions implemented inside the file BeaconConnection.jslib
	/// </summary>
	public class BeaconConnectorAndroid : IBeaconConnector
	{
		private AndroidJavaObject _beaconWalletViewModel = new AndroidJavaObject("it.airgap.beaconsdk.dapp.WalletViewModel");
		private AndroidJavaObject _beaconDAppViewModel = new AndroidJavaObject("it.airgap.beaconsdk.dapp.DAppViewModel");

		public void SetNetwork(string network, string rpc)
		{
		}

		public string GetActiveAccountAddress()
		{ 
			return _beaconDAppViewModel.Call<string>("getActiveAccountAddress");
		}

		public void ConnectAccount()
		{
			_beaconDAppViewModel.Call("startBeacon");
		}

		public void DisconectAccount()
		{
			_beaconDAppViewModel.Call("stopBeacon");
		}

		public void RequestTezosPermission(string networkName = "", string networkRPC = "")
		{
			_beaconDAppViewModel.Call("requestTezosPermission", networkName, networkRPC);
		}

		public void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null, ulong amount = 0, string networkName = "", string networkRPC = "")
		{
			_beaconDAppViewModel.Call("requestTezosOperation", destination, $"{amount}", entryPoint, arg, networkName, networkRPC);
		}

		public void RequestTezosSignPayload(int signingType, string payload)
		{
			_beaconDAppViewModel.Call("requestTezosSignPayload", signingType, payload);
		}

		public void RequestTezosBroadcast(string signedTransaction, string networkName = "", string networkRPC = "")
		{
			_beaconDAppViewModel.Call("requestTezosBroadcast", signedTransaction, networkName, networkRPC);
		}

		public void DisconnectAccount()
		{
			_beaconDAppViewModel.Call("stopBeacon");
		}

		public void RequestHandshake()
		{
			_beaconDAppViewModel.Call("requestHandshake");
		}
	}
}
#endif
