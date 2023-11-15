using System;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TezosSDK.Tezos;

namespace TezosSDK.Beacon
{

	public class WalletEventManager : MonoBehaviour
	{
		// Define private fields to hold the delegate lists for each event type.
		private Action<AccountInfo> _accountConnected;
		private Action<AccountInfo> _accountDisconnected;
		private Action<OperationResult> _contractCallCompleted;
		private Action<ErrorInfo> _accountConnectionFailed;
		private Action<ErrorInfo> _contractCallFailed;
		private Action<SignResult> _payloadSigned;
		private Action<OperationResult> _contractCallInjected;
		private Action<HandshakeData> _handshakeReceived;
		private Action<PairingDoneData> _pairingCompleted;

		private void HandleHandshakeReceived(string data)
		{
			var handshakeData = JsonUtility.FromJson<HandshakeData>(data);
			_handshakeReceived?.Invoke(handshakeData); // Invoke the event with the object.
		}

		[Serializable]
		public class PairingDoneData
		{
			public string DAppPublicKey; // Public key of the paired DApp
			public string Timestamp; // Timestamp to be possibly used for logging actions
		}

		// Central method that delegates the received event data to specific handlers.
		public void HandleEvent(string jsonEventData)
		{
			try
			{
				var eventData = JsonUtility.FromJson<UnifiedEvent>(jsonEventData);

				switch (eventData.EventType)
				{
					case "AccountConnected":
						HandleAccountConnected(eventData.Data);
						break;
					case "AccountDisconnected":
						HandleAccountDisconnected(eventData.Data);
						break;
					case "ContractCallCompleted":
						HandleContractCallCompleted(eventData.Data);
						break;
					case "AccountConnectionFailed":
						HandleAccountConnectionFailed(eventData.Data);
						break;
					case "ContractCallFailed":
						HandleContractCallFailed(eventData.Data);
						break;
					case "PayloadSigned":
						HandlePayloadSigned(eventData.Data);
						break;
					case "PairingDone":
						HandlePairingCompleted(eventData.Data);
						break;
					case "ContractCallInjected":
						HandleContractCallInjected(eventData.Data);
						break;
					case "HandshakeReceived":
						HandleHandshakeReceived(eventData.Data);
						break;
					default:
						Debug.LogWarning($"Unhandled event type: {eventData.EventType}");
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.LogError($"Error parsing event data: {ex.Message}\nData: {jsonEventData}");
			}
		}

		/// <summary>
		/// Triggered when an account is successfully connected.
		/// The returned string contains JSON-formatted account information.
		/// </summary>
		public event Action<AccountInfo> AccountConnected
		{
			add
			{
				if (_accountConnected == null || !_accountConnected.GetInvocationList().Contains(value))
					_accountConnected += value;
			}
			remove => _accountConnected -= value;
		}

		/// <summary>
		/// Triggered when an account connection attempt fails.
		/// The returned string contains JSON-formatted account information.
		/// </summary>
		public event Action<ErrorInfo> AccountConnectionFailed
		{
			add
			{
				if (_accountConnectionFailed == null || !_accountConnectionFailed.GetInvocationList().Contains(value))
					_accountConnectionFailed += value;
			}
			remove => _accountConnectionFailed -= value;
		}

		/// <summary>
		/// Triggered when an account is disconnected.
		/// The returned string is empty. TODO: Add JSON-formatted account information as return string.
		/// </summary>
		public event Action<AccountInfo> AccountDisconnected
		{
			add
			{
				if (_accountDisconnected == null || !_accountDisconnected.GetInvocationList().Contains(value))
					_accountDisconnected += value;
			}
			remove => _accountDisconnected -= value;
		}

		/// <summary>
		/// Triggered when a contract call is completed.
		/// The returned string is a JSON object containing the transaction hash and a success status.
		/// </summary>
		public event Action<OperationResult> ContractCallCompleted
		{
			add
			{
				if (_contractCallCompleted == null || !_contractCallCompleted.GetInvocationList().Contains(value))
					_contractCallCompleted += value;
			}
			remove => _contractCallCompleted -= value;
		}

		/// <summary>
		/// Triggered when a contract call is injected into the blockchain.
		/// The returned string is a JSON object containing the transaction hash and a success status.
		/// </summary>
		public event Action<OperationResult> ContractCallInjected
		{
			add
			{
				if (_contractCallInjected == null || !_contractCallInjected.GetInvocationList().Contains(value))
					_contractCallInjected += value;
			}
			remove => _contractCallInjected -= value;
		}

		/// <summary>
		/// Triggered when a contract call fails.
		/// The returned string is a JSON-formatted object of type ContractCallInjectionResult
		/// TODO: "result is error or empty" - please clarify the error type and format and if empty on success
		/// </summary>
		public event Action<ErrorInfo> ContractCallFailed
		{
			add
			{
				if (_contractCallFailed == null || !_contractCallFailed.GetInvocationList().Contains(value))
					_contractCallFailed += value;
			}
			remove => _contractCallFailed -= value;
		}

		/// <summary>
		/// Triggered when a payload is signed.
		/// The returned string is a JSON string of the payload signing result.
		/// </summary>
		public event Action<SignResult> PayloadSigned
		{
			add
			{
				if (_payloadSigned == null || !_payloadSigned.GetInvocationList().Contains(value))
					_payloadSigned += value;
			}
			remove => _payloadSigned -= value;
		}

		/// <summary>
		/// Triggered when a handshake is received.
		/// The returned string is a serialized summary of the pairing request,
		/// including app details like name, relay servers, and URLs.
		/// </summary>
		public event Action<HandshakeData> HandshakeReceived
		{
			add
			{
				if (_handshakeReceived == null || !_handshakeReceived.GetInvocationList().Contains(value))
					_handshakeReceived += value;
			}
			remove => _handshakeReceived -= value;
		}

		/// <summary>
		/// Invoked when the pairing process with a DApp is successfully completed.
		/// The returned string is a JSON-formatted object containing key details about the pairing, 
		/// including the DApp's name, URL, the network type, RPC URL, and the timestamp of the pairing completion.
		/// TODO: If not called from WebGL and if it will stay that way, warn users?
		/// </summary>
		public event Action<PairingDoneData> PairingCompleted
		{
			add
			{
				if (_pairingCompleted == null || !_pairingCompleted.GetInvocationList().Contains(value))
					_pairingCompleted += value;
			}
			remove => _pairingCompleted -= value;
		}

		private void HandlePairingCompleted(string data)
		{
			var pairingDoneData = JsonUtility.FromJson<PairingDoneData>(data);
			_pairingCompleted?.Invoke(pairingDoneData);
		}

		private void HandleAccountConnected(string data)
		{
			var accountInfo = JsonUtility.FromJson<AccountInfo>(data);
			_accountConnected?.Invoke(accountInfo);
		}

		private void HandleAccountConnectionFailed(string data)
		{
			var errorInfo = JsonUtility.FromJson<ErrorInfo>(data);
			_accountConnectionFailed?.Invoke(errorInfo);
		}

		private void HandleAccountDisconnected(string data)
		{
			var accountInfo = JsonUtility.FromJson<AccountInfo>(data);
			_accountDisconnected?.Invoke(accountInfo);
		}

		private void HandleContractCallCompleted(string data)
		{
			var operationResult = JsonUtility.FromJson<OperationResult>(data);
			_contractCallCompleted?.Invoke(operationResult);
		}

		private void HandleContractCallInjected(string data)
		{
			var operationResult = JsonUtility.FromJson<OperationResult>(data);
			_contractCallInjected?.Invoke(operationResult);
		}

		private void HandleContractCallFailed(string data)
		{
			var errorInfo = JsonUtility.FromJson<ErrorInfo>(data);
			_contractCallFailed?.Invoke(errorInfo);
		}

		private void HandlePayloadSigned(string data)
		{
			var signResult = JsonUtility.FromJson<SignResult>(data);
			_payloadSigned?.Invoke(signResult);
		}

		public IEnumerator TrackTransaction(string transactionHash)
		{
			var success = false;
			const float timeout = 30f; // seconds
			var timestamp = Time.time;

			// keep making requests until time out or success
			while (!success && Time.time - timestamp < timeout)
			{
				Logger.LogDebug($"Checking tx status: {transactionHash}");
                
                yield return TezosManager
					.Instance
					.Tezos
					.API
					.GetOperationStatus(result =>
					{
						if (result != null)
							success = JsonSerializer.Deserialize<bool>(result);
					}, transactionHash);

				yield return new WaitForSecondsRealtime(3);
			}

			var operationResult = new OperationResult
			{
				TransactionHash = transactionHash,
				IsSuccessful = true
			};

			var eventData = new UnifiedEvent
			{
				EventType = "ContractCallCompleted",
				Data = JsonUtility.ToJson(operationResult)
			};
			
			HandleEvent(JsonUtility.ToJson(eventData));
		}

	}

	[Serializable]
	public class HandshakeData
	{
		public string PairingData; // The data needed for the user to complete the pairing, like a QR code or deep link
	}

	[Serializable]
	public class AccountInfo
	{
		public string Address;
		public string PublicKey;
	}

	[Serializable]
	public class ErrorInfo
	{
		public string Message;
	}

	[Serializable]
	public class OperationResult
	{
		public string TransactionHash;
		public bool IsSuccessful;
		public string Error;
	}

	[Serializable]
	public class SignResult
	{
		public string Signature;
	}

	[Serializable]
	public class UnifiedEvent
	{
		public string EventType;
		public string Data; // JSON string, to be parsed further in specific handlers
	}
	
	

}