#if UNITY_IOS
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace BeaconSDK
{
	public class BeaconConnectorIOS : IBeaconConnector
	{
		#region ObjC Beacon interface

		[DllImport("__Internal")]
		private static extern void _IOSConnectAccount();

		[DllImport("__Internal")]
		private static extern void _IOSPair();

		[DllImport("__Internal")]
		private static extern void _IOSDisconnectAccount();

		[DllImport("__Internal")]
		private static extern void _IOSGetActiveAccount();

		[DllImport("__Internal")]
		private static extern void _IOSRequestTezosPermission(
			string networkName,
			string networkRPC
		);

		[DllImport("__Internal")]
		private static extern void _IOSRequestTezosOperation(
			string destination,
			string entryPoint,
			string arg,
			string amount,
			string networkName,
			string networkRPC
		);

		[DllImport("__Internal")]
		private static extern void _IOSRequestTezosSignPayload(
			int signingType,
			string payload
		);

		[DllImport("__Internal")]
		private static extern void _IOSRequestTezosBroadcast(
			string signedTransaction,
			string networkName,
			string networkRPC
		);

		#endregion

		#region ObjC to C# callback


		private unsafe delegate void ReceiveMessageDelegate(int type, char* encodedMessage, int length);

		[DllImport("__Internal")]
		private static extern void _RegisterReceiveMessageCallback(ReceiveMessageDelegate callback);

		#endregion

		public enum NativeMessageType
		{
			Log = 1,
			Handshake,
			Account,
			Signature,
			PublicKey,
			Pairing,
            Operation
		}

		private static string _activeAccount;
		
		private static BeaconMessageReceiver _messageReceiver;

		#region IBeaconConnector

		public void ConnectAccount()
		{
			if (_activeAccount != null)
				return;
			
			unsafe
			{
				_RegisterReceiveMessageCallback(ReceiveMessage);
			}
			
			_messageReceiver.AccountReceived += OnAccountReceived;
			_messageReceiver.AccountConnected += OnPermissionsReceived;

			_IOSConnectAccount();
		//	_IOSGetActiveAccount();
		}

		private void OnAccountReceived(string account)
		{
			var json = JsonSerializer.Deserialize<JsonElement>(account);    
			_activeAccount = json.GetProperty("address").GetString();       
			Debug.Log("my address: " + _activeAccount);                     
		}

		private void OnPermissionsReceived(string account)
		{
			var json = JsonSerializer.Deserialize<JsonElement>(account);   
			if (json.TryGetProperty("account", out json))                  
			{                                                              
				_activeAccount = json.GetProperty("address").GetString();  
			}                                                              
		}

		public string GetActiveAccountAddress()
		{
			// TODO: make _IOSGetActiveAccount to synchronously return a string
			_IOSGetActiveAccount(); // async
			return _activeAccount;
		}

		public void DisconnectAccount()
		{
			_messageReceiver.AccountReceived -= OnAccountReceived;            
			_messageReceiver.AccountConnected -= OnPermissionsReceived;       
			
			_activeAccount = null;
			_IOSDisconnectAccount();
		}

		public void SetNetwork(string network, string rpc) {}
		public void SwitchAccounts() {}

		public void SetBeaconMessageReceiver(BeaconMessageReceiver messageReceiver)
		{
			_messageReceiver = messageReceiver;
		}

		public void RequestHandshake() => _IOSPair();

		#endregion
		
		// has to be static in order to pass callback to native
		[AOT.MonoPInvokeCallback(typeof(ReceiveMessageDelegate))]
		static unsafe void ReceiveMessage(int type, char* str, int length)
		{
			// alternatively to sending length, one could Trim the new string from 0 to '\0' symbol 
			var msg = Encoding.UTF8.GetString((byte*)str, length);

			switch ((NativeMessageType)type)
			{
				case NativeMessageType.Log:
					Debug.Log("-- Log -- " + msg);
					break;
				case NativeMessageType.Handshake:
					Debug.Log("-- Handshake -- " + msg);
					_messageReceiver.OnHandshakeReceived(msg);
					break;
				case NativeMessageType.Account:
					Debug.Log("-- Account -- " + msg);
				//	_activeAccount = msg;
					break;
				case NativeMessageType.Signature:
					Debug.Log("-- Signature -- " + msg + " of length:" + length);
					_messageReceiver.OnPayloadSigned(msg);
					break;
				case NativeMessageType.PublicKey:
					Debug.Log("-- Public key -- " + msg);
					_messageReceiver.OnAccountConnected(msg);
					break;
				case NativeMessageType.Pairing: 
					Debug.Log("-- Pairing complete -- " + msg);
					_messageReceiver.OnPairingCompleted(msg);
					break;
				case NativeMessageType.Operation:
					Debug.Log("-- Operation complete -- " + msg);
					_messageReceiver.OnContractCallCompleted(msg);
					break;
			}
		}
	
		public void RequestTezosPermission(string networkName = "", string networkRPC = "")
		{
			_IOSRequestTezosPermission(networkName, networkRPC);
		}

		public void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null,
			ulong amount = 0, string networkName = "", string networkRPC = "")
		{
			_IOSRequestTezosOperation(destination, entryPoint, arg, amount.ToString(), networkName, networkRPC);
		}

		public void RequestTezosSignPayload(int signingType, string payload)
		{
			_IOSRequestTezosSignPayload(signingType, payload);
		}

		public void RequestTezosBroadcast(string signedTransaction, string networkName = "", string networkRPC = "")
		{
			_IOSRequestTezosBroadcast(signedTransaction, networkName, networkRPC);
		}
	}
}
#endif
