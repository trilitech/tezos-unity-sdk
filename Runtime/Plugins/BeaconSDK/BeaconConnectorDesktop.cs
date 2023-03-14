#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine;

namespace BeaconSDK
{
	/// <summary>
	/// Android implementation of the BeaconConnector.
	/// Binds the functions implemented inside the file BeaconConnection.jslib
	/// </summary>
	public class BeaconConnectorDesktop : IBeaconConnector
	{
		private AndroidJavaObject _beaconWalletViewModel = new AndroidJavaObject("it.airgap.beaconsdk.dapp.WalletViewModel");
		private AndroidJavaObject _beaconDAppViewModel = new AndroidJavaObject("it.airgap.beaconsdk.dapp.DAppViewModel");

		public static string NativeMessage = string.Empty;

		// private QRCodeView _qrCodeView;
		private BeaconMessageReceiver _messageReceiver;

		public void SetNetwork(string network, string rpc)
		{
			throw new System.NotImplementedException();

			//network = "jakartanet";
			//rpc = "https://rpc.tzkt.io/jakartanet";
			//SetNetwork(network, rpc);
		}

		public string GetActiveAccount()
		{
			string account = _beaconDAppViewModel.Call<string>("checkForActiveAccount");
			Debug.LogError($"Account spits out -=-{account}-=-");

			return account;
		}

		public string GetActiveAccountAddress()
		{ 
			string address = _beaconDAppViewModel.Call<string>("getActiveAccountAddress");
			Debug.LogError($"Address spits out -=-{address}-=-");

			return address;
		}

		// public void SetQRCodeView(QRCodeView qRCodeView)
		// {
		// 	_qrCodeView = qRCodeView;
		// }

		public void SetBeaconMessageReceiver(BeaconMessageReceiver messageReceiver)
		{
			_messageReceiver = messageReceiver;
		}

		public void ConnectAccount()
		{
			_beaconDAppViewModel.Call("startBeacon");
		}

		public void DisconectAccount()
		{
			_beaconDAppViewModel.Call("stopBeacon");
		}

		public void PauseBeacon()
		{
			_beaconDAppViewModel.Call("pauseBeacon");
		}

		public void ResumeBeacon()
		{
			_beaconDAppViewModel.Call("resumeBeacon");
		}

		// public void QRCode(string handshake)
		// {
		// 	_qrCodeView.SetQrCode(_beaconDAppViewModel.Call<string>("pair"));
		// }

		public void Unpair()
		{
			_beaconDAppViewModel.Call("reset");
		}

		public void SendRequestPermissions()
		{
			Debug.Log(_beaconDAppViewModel.Call<string>("requestPermission"));
		}

		public void SendResponse()
		{
			//Now done somewhere else
			throw new System.NotImplementedException();
		}

		public void CallContract(string destination, string entryPoint, string arg, long amount = 0)
		{
			entryPoint = "main";
			amount = 0;
			//destination = "KT1E4xgc9iniojkZqs1BDs117bzaYfMHZcPs"; // my structs contract
			//arg = "(PAIR \"str\" 2)";
			destination = "KT1DCcniV9tatQFVLnPv15i4kGYNgpdE6GhS"; // my counter contract
			arg = "8";
			throw new System.NotImplementedException();
			//AndSendContractCall(destination, amount.ToString(), entryPoint, arg);
		}

		public void DisconnectAccount()
		{
			_beaconDAppViewModel.Call("stopBeacon");
		}

		public void SendMutez(long amount, string address)
		{
			_beaconDAppViewModel.Call<string>("requestOperation", address, $"{amount}", "default", null);
		}

		public void SwitchAccounts()
		{
			throw new System.NotImplementedException();
		}

		public void Reset()
		{
			_beaconDAppViewModel.Call("stopBeacon");
		}

		public void RequestTezosPermission(string networkName = "", string networkRPC = "")
		{
			throw new System.NotImplementedException();
		}

		public void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null,
			ulong amount = 0, string networkName = "", string networkRPC = "")
		{
			throw new System.NotImplementedException();
		}

		public void RequestTezosSignPayload(int signingType, string payload)
		{
			throw new System.NotImplementedException();
		}

		public void RequestTezosBroadcast(string signedTransaction, string networkName = "", string networkRPC = "")
		{
			throw new System.NotImplementedException();
		}

		public void RequestHandshake()
		{
			throw new System.NotImplementedException();
		}
	}
}
#endif
