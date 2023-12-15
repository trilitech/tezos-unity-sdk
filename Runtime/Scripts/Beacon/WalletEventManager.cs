using System;
using System.Linq;
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
		public const string EventTypeAccountConnected = "AccountConnected";
		public const string EventTypeAccountConnectionFailed = "AccountConnectionFailed";
		public const string EventTypeAccountDisconnected = "AccountDisconnected";
		public const string EventTypeContractCallCompleted = "ContractCallCompleted";
		public const string EventTypeContractCallFailed = "ContractCallFailed";
		public const string EventTypeContractCallInjected = "ContractCallInjected";
		public const string EventTypeHandshakeReceived = "HandshakeReceived";
		public const string EventTypePairingDone = "PairingDone";
		public const string EventTypePayloadSigned = "PayloadSigned";

		private Action<AccountInfo> accountConnected;
		private Action<ErrorInfo> accountConnectionFailed;
		private Action<AccountInfo> accountDisconnected;
		private Action<OperationResult> contractCallCompleted;
		private Action<ErrorInfo> contractCallFailed;
		private Action<OperationResult> contractCallInjected;
		private Action<HandshakeData> handshakeReceived;
		private Action<PairingDoneData> pairingCompleted;
		private Action<SignResult> payloadSigned;

		/// <summary>
		///     Runs when an account connects successfully. Provides the account information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="AccountInfo" /> object containing the address and public key of the connected account.
		///     It is triggered in response to a successful connection action from the wallet.
		/// </remarks>
		public event Action<AccountInfo> AccountConnected
		{
			add
			{
				if (accountConnected == null || !accountConnected.GetInvocationList().Contains(value))
				{
					accountConnected += value;
				}
			}
			remove => accountConnected -= value;
		}

		/// <summary>
		///     Runs when a connection to an account fails. Provides error information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="ErrorInfo" /> object containing the error message of the failed connection attempt.
		///     It is triggered when a connection attempt to an account encounters an error.
		/// </remarks>
		public event Action<ErrorInfo> AccountConnectionFailed
		{
			add
			{
				if (accountConnectionFailed == null || !accountConnectionFailed.GetInvocationList().Contains(value))
				{
					accountConnectionFailed += value;
				}
			}
			remove => accountConnectionFailed -= value;
		}

		/// <summary>
		///     Runs when an account disconnects successfully. Provides the account information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="AccountInfo" /> object containing the address and public key of the disconnected account.
		///     It is triggered in response to a successful disconnection action from the wallet.
		/// </remarks>
		public event Action<AccountInfo> AccountDisconnected
		{
			add
			{
				if (accountDisconnected == null || !accountDisconnected.GetInvocationList().Contains(value))
				{
					accountDisconnected += value;
				}
			}
			remove => accountDisconnected -= value;
		}

		/// <summary>
		///     Runs when a call to a smart contract is confirmed on the blockchain. Provides the result of the call.
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
				if (contractCallCompleted == null || !contractCallCompleted.GetInvocationList().Contains(value))
				{
					contractCallCompleted += value;
				}
			}
			remove => contractCallCompleted -= value;
		}

		/// <summary>
		///     Runs when a call to a smart contract fails. Provides error details.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="ErrorInfo" /> object containing the error message of the failed contract call.
		///     It is triggered when a contract call attempted by the wallet encounters an error.
		/// </remarks>
		public event Action<ErrorInfo> ContractCallFailed
		{
			add
			{
				if (contractCallFailed == null || !contractCallFailed.GetInvocationList().Contains(value))
				{
					contractCallFailed += value;
				}
			}
			remove => contractCallFailed -= value;
		}

		/// <summary>
		///     Runs when a call to a smart contract is sent to Tezos but before it has been included in a block and confirmed. Provides the hash of the transaction.
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
				if (contractCallInjected == null || !contractCallInjected.GetInvocationList().Contains(value))
				{
					contractCallInjected += value;
				}
			}
			remove => contractCallInjected -= value;
		}

		/// <summary>
		///     Runs when a handshake with a user's wallet application is received. Provides the handshake details.
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
				if (handshakeReceived == null || !handshakeReceived.GetInvocationList().Contains(value))
				{
					handshakeReceived += value;
				}
			}
			remove => handshakeReceived -= value;
		}

		/// <summary>
		///     Runs when the user's wallet is connected but before the user has approved the connection in the wallet app. Provides details of the pairing completion.
		///     Note: This event is not supported in WebGL builds.
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
				#if UNITY_WEBGL
					Logger.LogWarning("PairingCompleted event is not supported in WebGL builds.");
				#endif
				
				if (pairingCompleted == null || !pairingCompleted.GetInvocationList().Contains(value))
				{
					pairingCompleted += value;
				}
			}
			remove => pairingCompleted -= value;
		}

		/// <summary>
		///     Runs when the user signs a payload. Provides the sign result.
		/// </summary>
		/// <remarks>
		///     Provides a <see cref="SignResult" /> object containing the signature value.
		///     The event is triggered in response to a successful payload signing request.
		/// </remarks>
		public event Action<SignResult> PayloadSigned
		{
			add
			{
				if (payloadSigned == null || !payloadSigned.GetInvocationList().Contains(value))
				{
					payloadSigned += value;
				}
			}
			remove => payloadSigned -= value;
		}

		/// <summary>
		///     Processes the incoming JSON event data and dispatches it to the corresponding event based on the event type.
		/// </summary>
		/// <param name="jsonEventData">
		///     The JSON string representing the event data, which includes the event type and associated
		///     data.
		/// </param>
		/// <remarks>
		///     The method decodes the JSON event data, identifies the type of event, and invokes the corresponding
		///     event handler with the proper deserialized event data object.
		/// </remarks>
		public void HandleEvent(string jsonEventData)
		{
			try
			{
				var eventData = JsonUtility.FromJson<UnifiedEvent>(jsonEventData);

				switch (eventData.EventType)
				{
					case EventTypeHandshakeReceived:
						HandleEvent(eventData.Data, handshakeReceived);
						break;
					case EventTypePairingDone:
						HandleEvent(eventData.Data, pairingCompleted);
						break;
					case EventTypeAccountConnected:
						HandleEvent(eventData.Data, accountConnected);
						break;
					case EventTypeAccountConnectionFailed:
						HandleEvent(eventData.Data, accountConnectionFailed);
						break;
					case EventTypeAccountDisconnected:
						HandleEvent(eventData.Data, accountDisconnected);
						break;
					case EventTypeContractCallInjected:
						HandleEvent(eventData.Data, contractCallInjected);
						break;
					case EventTypeContractCallCompleted:
						HandleEvent(eventData.Data, contractCallCompleted);
						break;
					case EventTypeContractCallFailed:
						HandleEvent(eventData.Data, contractCallFailed);
						break;
					case EventTypePayloadSigned:
						HandleEvent(eventData.Data, payloadSigned);
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
		///     Deserializes the event data and invokes the corresponding event delegate.
		/// </summary>
		/// <typeparam name="T">The type of the event data object.</typeparam>
		/// <param name="data">The JSON string representing the event data to be deserialized.</param>
		/// <param name="eventAction">The action delegate to invoke with the deserialized event data.</param>
		/// <remarks>
		///     This method deserializes the JSON string into an object of type <typeparamref name="T" />
		///     and then invokes the provided delegate <paramref name="eventAction" /> with the deserialized object.
		///     It is designed to be called within a switch-case structure that handles each specific event type.
		/// </remarks>
		private void HandleEvent<T>(string data, Action<T> eventAction)
		{
			var deserializedData = JsonUtility.FromJson<T>(data);
			eventAction?.Invoke(deserializedData);
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