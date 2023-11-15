using System;
using System.Collections;
using System.Linq;
using System.Text.Json;
using TezosSDK.Tezos;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Beacon
{

	/// <summary>
	///     Manages event propagation within a Unity environment for wallet-related actions specific to the Tezos blockchain.
	///     Handles incoming JSON event data and triggers corresponding C# events to handle these wallet actions.
	/// </summary>
	public class WalletEventManager : MonoBehaviour
	{
		// Define private fields to hold the delegate lists for each event type.
		private Action<AccountInfo> _accountConnected;
		private Action<ErrorInfo> _accountConnectionFailed;
		private Action<AccountInfo> _accountDisconnected;
		private Action<OperationResult> _contractCallCompleted;
		private Action<ErrorInfo> _contractCallFailed;
		private Action<OperationResult> _contractCallInjected;
		private Action<HandshakeData> _handshakeReceived;
		private Action<PairingDoneData> _pairingCompleted;
		private Action<SignResult> _payloadSigned;

		/// <summary>
		///     Occurs when an account connected successfully. Provides the account information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="AccountInfo" /> object containing the address and public key of the connected account.
		///     It is triggered in response to a successful connection action from the wallet.
		/// </remarks>
		public event Action<AccountInfo> AccountConnected
		{
			add
			{
				if (_accountConnected == null || !_accountConnected.GetInvocationList().Contains(value))
				{
					_accountConnected += value;
				}
			}
			remove => _accountConnected -= value;
		}

		/// <summary>
		///     Occurs when the connection to an account failed. Provides error information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="ErrorInfo" /> object containing the error message of the failed connection attempt.
		///     It is triggered when a connection attempt to an account encounters an error.
		/// </remarks>
		public event Action<ErrorInfo> AccountConnectionFailed
		{
			add
			{
				if (_accountConnectionFailed == null || !_accountConnectionFailed.GetInvocationList().Contains(value))
				{
					_accountConnectionFailed += value;
				}
			}
			remove => _accountConnectionFailed -= value;
		}

		/// <summary>
		///     Occurs when an account disconnected successfully. Provides the account information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="AccountInfo" /> object containing the address and public key of the disconnected account.
		///     It is triggered in response to a successful disconnection action from the wallet.
		/// </remarks>
		public event Action<AccountInfo> AccountDisconnected
		{
			add
			{
				if (_accountDisconnected == null || !_accountDisconnected.GetInvocationList().Contains(value))
				{
					_accountDisconnected += value;
				}
			}
			remove => _accountDisconnected -= value;
		}

		/// <summary>
		///     Occurs when a contract call completed successfully. Provides the result of the call.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="OperationResult" /> object with the transaction hash and success status.
		///     This event is fired when a blockchain operation completes and has been injected into the blockchain.
		///     The result includes the transaction hash and an indication of whether it was successful.
		/// </remarks>
		public event Action<OperationResult> ContractCallCompleted
		{
			add
			{
				if (_contractCallCompleted == null || !_contractCallCompleted.GetInvocationList().Contains(value))
				{
					_contractCallCompleted += value;
				}
			}
			remove => _contractCallCompleted -= value;
		}

		/// <summary>
		///     Occurs when a contract call fails. Provides error details.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="ErrorInfo" /> object containing the error message of the failed contract call.
		///     It is triggered when a contract call attempted by the wallet encounters an error.
		/// </remarks>
		public event Action<ErrorInfo> ContractCallFailed
		{
			add
			{
				if (_contractCallFailed == null || !_contractCallFailed.GetInvocationList().Contains(value))
				{
					_contractCallFailed += value;
				}
			}
			remove => _contractCallFailed -= value;
		}

		/// <summary>
		///     Occurs when a contract call is injected into the blockchain. Provides the result of the injection.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="OperationResult" /> object containing the transaction hash and success status after the
		///     contract call is injected.
		///     It is triggered after an operation request (like a contract call) is sent successfully to the blockchain network.
		/// </remarks>
		public event Action<OperationResult> ContractCallInjected
		{
			add
			{
				if (_contractCallInjected == null || !_contractCallInjected.GetInvocationList().Contains(value))
				{
					_contractCallInjected += value;
				}
			}
			remove => _contractCallInjected -= value;
		}

		/// <summary>
		///     Occurs when handshake data is received. Provides the handshake details.
		/// </summary>
		/// <remarks>
		///     Provides a <see cref="HandshakeData" /> object with the pairing information required for user completion.
		///     This event is triggered as part of the pairing process. The handshake data may include a QR code or other
		///     information necessary to complete pairing with a DApp.
		/// </remarks>
		public event Action<HandshakeData> HandshakeReceived
		{
			add
			{
				if (_handshakeReceived == null || !_handshakeReceived.GetInvocationList().Contains(value))
				{
					_handshakeReceived += value;
				}
			}
			remove => _handshakeReceived -= value;
		}

		/// <summary>
		///     Occurs when the pairing process with a DApp is completed successfully. Provides details of the pairing completion.
		/// </summary>
		/// <remarks>
		///     Provides a <see cref="PairingDoneData" /> object with details about the pairing, such as the DApp's public key and
		///     the timestamp of pairing completion.
		///     Triggered when the pairing between the Tezos wallet and a DApp is completed. Reveals public key and a timestamp
		///     indicating when the pairing was finalized.
		/// </remarks>
		/// TODO: If not called from WebGL and if it will stay that way, warn users? Or remove completely? Is this needed?
		public event Action<PairingDoneData> PairingCompleted
		{
			add
			{
				if (_pairingCompleted == null || !_pairingCompleted.GetInvocationList().Contains(value))
				{
					_pairingCompleted += value;
				}
			}
			remove => _pairingCompleted -= value;
		}

		/// <summary>
		///     Occurs when a payload has been signed. Provides the sign result.
		/// </summary>
		/// <remarks>
		///     Provides a <see cref="SignResult" /> object containing the signature value.
		///     The event is triggered in response to a successful payload signing request.
		/// </remarks>
		public event Action<SignResult> PayloadSigned
		{
			add
			{
				if (_payloadSigned == null || !_payloadSigned.GetInvocationList().Contains(value))
				{
					_payloadSigned += value;
				}
			}
			remove => _payloadSigned -= value;
		}

		/// <summary>
		///     Processes the incoming JSON event data and routes it to the corresponding event handler based on the event type.
		/// </summary>
		/// <param name="jsonEventData">The JSON string representing the event data, which includes the type and associated data.</param>
		/// <remarks>
		///     This method is the central point for event processing within the WalletEventManager system. It parses the incoming
		///     JSON event data to extract the event type and data, then dispatches the event by invoking the appropriate specific
		///     handler method for each event type.
		/// </remarks>
		public void HandleEvent(string jsonEventData)
		{
			try
			{
				var eventData = JsonUtility.FromJson<UnifiedEvent>(jsonEventData);

				switch (eventData.EventType)
				{
					case "HandshakeReceived":
						HandleHandshakeReceived(eventData.Data);
						break;
					case "PairingDone":
						HandlePairingCompleted(eventData.Data);
						break;
					case "AccountConnected":
						HandleAccountConnected(eventData.Data);
						break;
					case "AccountDisconnected":
						HandleAccountDisconnected(eventData.Data);
						break;
					case "AccountConnectionFailed":
						HandleAccountConnectionFailed(eventData.Data);
						break;
					case "ContractCallInjected":
						HandleContractCallInjected(eventData.Data);
						break;
					case "ContractCallCompleted":
						HandleContractCallCompleted(eventData.Data);
						break;
					case "ContractCallFailed":
						HandleContractCallFailed(eventData.Data);
						break;
					case "PayloadSigned":
						HandlePayloadSigned(eventData.Data);
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

		public IEnumerator TrackTransaction(string transactionHash)
		{
			var success = false;
			const float timeout = 30f; // seconds
			var timestamp = Time.time;

			// keep making requests until time out or success
			while (!success && Time.time - timestamp < timeout)
			{
				Logger.LogDebug($"Checking tx status: {transactionHash}");

				yield return TezosManager.Instance.Tezos.API.GetOperationStatus(result =>
				{
					if (result != null)
					{
						success = JsonSerializer.Deserialize<bool>(result);
					}
				}, transactionHash);

				yield return new WaitForSecondsRealtime(3);
			}

			var operationResult = new OperationResult
			{
				TransactionHash = transactionHash
			};

			var eventData = new UnifiedEvent
			{
				EventType = "ContractCallCompleted",
				Data = JsonUtility.ToJson(operationResult)
			};

			HandleEvent(JsonUtility.ToJson(eventData));
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

		private void HandleContractCallFailed(string data)
		{
			var errorInfo = JsonUtility.FromJson<ErrorInfo>(data);
			_contractCallFailed?.Invoke(errorInfo);
		}

		private void HandleContractCallInjected(string data)
		{
			var operationResult = JsonUtility.FromJson<OperationResult>(data);
			_contractCallInjected?.Invoke(operationResult);
		}

		private void HandleHandshakeReceived(string data)
		{
			var handshakeData = JsonUtility.FromJson<HandshakeData>(data);
			_handshakeReceived?.Invoke(handshakeData); // Invoke the event with the object.
		}

		private void HandlePairingCompleted(string data)
		{
			var pairingDoneData = JsonUtility.FromJson<PairingDoneData>(data);
			_pairingCompleted?.Invoke(pairingDoneData);
		}

		private void HandlePayloadSigned(string data)
		{
			var signResult = JsonUtility.FromJson<SignResult>(data);
			_payloadSigned?.Invoke(signResult);
		}
	}

	/// <summary>
	///     Contains information related to the successful completion of a pairing between the Tezos wallet and a DApp.
	/// </summary>
	[Serializable]
	public class PairingDoneData
	{
		/// <summary>
		///     The public key of the paired DApp, which is used to identify the DApp on the Tezos blockchain.
		/// </summary>
		public string DAppPublicKey; // Public key of the paired DApp

		/// <summary>
		///     The timestamp indicating the exact time when the pairing was completed. This can be used for logging or
		///     verification purposes.
		/// </summary>
		public string Timestamp; // Timestamp to be possibly used for logging actions
	}

	/// <summary>
	///     Represents data required for the wallet to complete the pairing process with a decentralized application (DApp).
	/// </summary>
	[Serializable]
	public class HandshakeData
	{
		/// <summary>
		///     The data needed for the user to complete the pairing, which may be a QR code or deep link.
		/// </summary>
		public string PairingData;
	}

	/// <summary>
	///     Contains information about a Tezos account, including the address and public key.
	/// </summary>
	[Serializable]
	public class AccountInfo
	{
		/// <summary>
		///     The Tezos wallet address.
		/// </summary>
		public string Address;

		/// <summary>
		///     The public key associated with the Tezos wallet.
		/// </summary>
		public string PublicKey;
	}

	/// <summary>
	///     Contains error information, primarily used when an operation or connection attempt fails.
	/// </summary>
	[Serializable]
	public class ErrorInfo
	{
		/// <summary>
		///     The error message providing details about the failure.
		/// </summary>
		public string Message;
	}

	/// <summary>
	///     Contains the result of a blockchain operation, indicating its success and transaction details.
	/// </summary>
	[Serializable]
	public class OperationResult
	{
		/// <summary>
		///     The hash of the transaction associated with the operation.
		/// </summary>
		public string TransactionHash;
	}

	/// <summary>
	///     Contains the result of a payload signing operation.
	/// </summary>
	[Serializable]
	public class SignResult
	{
		/// <summary>
		///     The signature resulting from the payload signing operation.
		/// </summary>
		public string Signature;
	}

	/// <summary>
	///     Represents a general structure for an event containing its type and associated data.
	///     It is used to unify event messages for processing by event handlers.
	/// </summary>
	[Serializable]
	public class UnifiedEvent
	{
		/// <summary>
		///     Specifies the type of event.
		/// </summary>
		public string EventType;

		/// <summary>
		///     Contains the data associated with the event in a JSON string format,
		///     which is further parsed in specific event handlers.
		/// </summary>
		public string Data;
	}

}