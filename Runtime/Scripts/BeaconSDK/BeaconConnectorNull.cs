using Beacon.Sdk.Beacon.Sign;
using UnityEngine;

namespace BeaconSDK
{
	/// <summary>
	/// Null implementation of the Beacon Connector.
	/// This should be used when running on a platform that doesn't support
	/// a real connection to the BeaconSdk, for example the Unity Editor.
	/// </summary>
	public class BeaconConnectorNull : IBeaconConnector
	{
		public void ConnectAccount()
		{
			Debug.LogError("Platform not supported");
		}

		// public void SetQRCodeView(QRCodeView qRCodeView)
		// {
		// 	Debug.LogError("Platform not supported");
		// }
		
		public void SetBeaconMessageReceiver(BeaconMessageReceiver messageReceiver)
		{
			Debug.LogError("Platform not supported");
		}

		public void QRCode(string handshake)
		{
			Debug.LogError("Platform not supported");
		}

		public void Unpair()
		{
			Debug.LogError("Platform not supported");
		}

		public void DisconnectAccount()
		{
			Debug.LogError("Platform not supported");
		}

		public void SendRequestPermissions()
		{
			Debug.LogError("Platform not supported");
		}

		public void Reset()
		{
			Debug.LogError("Platform not supported");
		}

		public void CallContract(string destination, string entryPoint, string arg, long amount = 0)
		{
			Debug.LogError("Platform not supported");
		}

		public void SendMutez(long amount, string address)
		{
			Debug.LogError("Platform not supported");
		}

		public void SetNetwork(string network, string rpc)
		{
			Debug.LogError("Platform not supported");
		}

		public void SwitchAccounts()
		{
			Debug.LogError("Platform not supported");
		}

		public string GetActiveAccount()
		{
			Debug.LogError("Platform not supported");
			return string.Empty;
		}

		public string GetActiveAccountAddress()
		{
			Debug.LogError("Platform not supported");
			return "1234";
		}

		public void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null,
			ulong amount = 0, string networkName = "", string networkRPC = "")
		{
			throw new System.NotImplementedException();
		}

		public void RequestTezosSignPayload(SignPayloadType signingType, string payload)
		{
			throw new System.NotImplementedException();
		}

		public void RequestTezosBroadcast(string signedTransaction, string networkName = "", string networkRPC = "")
		{
			throw new System.NotImplementedException();
		}

		public void RequestTezosPermission(string networkName = "", string networkRPC = "")
		{
			throw new System.NotImplementedException();
		}

		public void RequestHandshake()
		{
			throw new System.NotImplementedException();
		}
	}
}