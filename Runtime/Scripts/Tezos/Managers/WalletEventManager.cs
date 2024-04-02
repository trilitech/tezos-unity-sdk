using System;
using System.Linq;
using Beacon.Sdk.Beacon;
using TezosSDK.Patterns;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

// ReSharper disable InconsistentNaming

namespace TezosSDK.Tezos
{

	/// <summary>
	///     Manages event propagation within a Unity environment for wallet-related actions specific to the Tezos blockchain.
	///     Handles incoming JSON event data and triggers corresponding C# events to handle these wallet actions.
	/// </summary>
	public class WalletEventManager : SingletonMonoBehaviour<WalletEventManager>, IWalletEventManager
	{
		public const string EventTypeHandshakeReceived = "HandshakeReceived";
		public const string EventTypeOperationCompleted = "OperationCompleted";
		public const string EventTypeOperationFailed = "OperationFailed";
		public const string EventTypeOperationInjected = "OperationInjected";
		public const string EventTypePairingDone = "PairingDone";
		public const string EventTypePayloadSigned = "PayloadSigned";
		public const string EventTypeSDKInitialized = "SDKInitialized";
		public const string EventTypeWalletConnected = "AccountConnected";
		public const string EventTypeWalletConnectionFailed = "AccountConnectionFailed";
		public const string EventTypeWalletDisconnected = "AccountDisconnected";

		public event Action SDKInitialized
		{
			add
			{
				if (sdkInitialized == null || !sdkInitialized.GetInvocationList().Contains(value))
				{
					sdkInitialized += value;
				}
			}
			remove => sdkInitialized -= value;
		}

		private event Action<HandshakeData> handshakeReceived;
		private event Action<OperationInfo> operationCompleted;
		private event Action<OperationInfo> operationFailed;
		private event Action<OperationInfo> operationInjected;
		private event Action<PairingDoneData> pairingCompleted;
		private event Action<SignResult> payloadSigned;
		private event Action sdkInitialized;
		private event Action<WalletInfo> walletConnected;
		private event Action<string> walletConnectionFailed;
		private event Action<WalletInfo> walletDisconnected;

		#region IWalletEventManager Implementation

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

		public event Action<OperationInfo> OperationCompleted
		{
			add
			{
				if (operationCompleted == null || !operationCompleted.GetInvocationList().Contains(value))
				{
					operationCompleted += value;
				}
			}
			remove => operationCompleted -= value;
		}

		public event Action<OperationInfo> OperationFailed
		{
			add
			{
				if (operationFailed == null || !operationFailed.GetInvocationList().Contains(value))
				{
					operationFailed += value;
				}
			}
			remove => operationFailed -= value;
		}

		public event Action<OperationInfo> OperationInjected
		{
			add
			{
				if (operationInjected == null || !operationInjected.GetInvocationList().Contains(value))
				{
					operationInjected += value;
				}
			}
			remove => operationInjected -= value;
		}

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

		public event Action<WalletInfo> WalletConnected
		{
			add
			{
				if (walletConnected == null || !walletConnected.GetInvocationList().Contains(value))
				{
					walletConnected += value;
				}
			}
			remove => walletConnected -= value;
		}

		public event Action<string> WalletConnectionFailed
		{
			add
			{
				if (walletConnectionFailed == null || !walletConnectionFailed.GetInvocationList().Contains(value))
				{
					walletConnectionFailed += value;
				}
			}
			remove => walletConnectionFailed -= value;
		}

		public event Action<WalletInfo> WalletDisconnected
		{
			add
			{
				if (walletDisconnected == null || !walletDisconnected.GetInvocationList().Contains(value))
				{
					walletDisconnected += value;
				}
			}
			remove => walletDisconnected -= value;
		}

		public void HandleEvent(UnifiedEvent unifiedEvent)
		{
			var jsonEventData = JsonUtility.ToJson(unifiedEvent);
			HandleEvent(jsonEventData);
		}

		#endregion

		public void DispatchSDKInitializedEvent()
		{
			var sdkInitializedEvent = new UnifiedEvent(EventTypeSDKInitialized);

			HandleEvent(sdkInitializedEvent);
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
		/// <remarks>
		///     JSON strings are used to enable communication between the Unity environment and WebGL builds.
		/// </remarks>
		private void HandleEvent(string jsonEventData)
		{
			try
			{
				var eventData = JsonUtility.FromJson<UnifiedEvent>(jsonEventData);

				switch (eventData.GetEventType())
				{
					case EventTypeHandshakeReceived:
						HandleEvent(eventData.GetData(), handshakeReceived);
						break;
					case EventTypePairingDone:
						HandleEvent(eventData.GetData(), pairingCompleted);
						break;
					case EventTypeWalletConnected:
						HandleEvent(eventData.GetData(), walletConnected);
						break;
					case EventTypeWalletConnectionFailed:
						HandleEvent(eventData.GetData(), walletConnectionFailed);
						break;
					case EventTypeWalletDisconnected:
						HandleEvent(eventData.GetData(), walletDisconnected);
						break;
					case EventTypeOperationInjected:
						HandleEvent(eventData.GetData(), operationInjected);
						break;
					case EventTypeOperationCompleted:
						HandleEvent(eventData.GetData(), operationCompleted);
						break;
					case EventTypeOperationFailed:
						HandleEvent(eventData.GetData(), operationFailed);
						break;
					case EventTypePayloadSigned:
						HandleEvent(eventData.GetData(), payloadSigned);
						break;
					case EventTypeSDKInitialized:
						sdkInitialized?.Invoke();
						break;
					default:
						Debug.LogWarning($"Unhandled event type: {eventData.GetEventType()}");
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
		private void HandleEvent<T>(object data, Action<T> eventAction)
		{
			var deserializedData = JsonUtility.FromJson<T>(data.ToString());
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
	public class WalletInfo
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

	// /// <summary>
	// ///     Contains error information, primarily used when an operation or connection attempt fails.
	// /// </summary>
	// [Serializable]
	// public class ErrorInfo
	// {
	// 	/// <summary>
	// 	///     The error message providing details about the failure.
	// 	/// </summary>
	// 	public string Message;
	// }

	/// <summary>
	///     Contains the result of a blockchain operation, indicating its success and transaction details.
	/// </summary>
	[Serializable]
	public class OperationInfo
	{
		/// <summary>
		///     The hash of the transaction associated with the operation.
		/// </summary>
		public string TransactionHash;

		/// <summary>
		///     The ID of the operation.
		/// </summary>
		public string Id;

		/// <summary>
		///     The type of operation.
		/// </summary>
		public BeaconMessageType OperationType;

		/// <summary>
		///     Conveys the error message if the operation failed.
		/// </summary>
		public string ErrorMesssage;

		public OperationInfo(
			string transactionHash,
			string id,
			BeaconMessageType operationType,
			string errorMessage = null)
		{
			TransactionHash = transactionHash;
			Id = id;
			OperationType = operationType;
			ErrorMesssage = errorMessage;
		}
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
		[SerializeField] private string EventType;
		[SerializeField] private string Data;

		/// <summary>
		///     Initializes a new instance of the <see cref="UnifiedEvent" /> class.
		/// </summary>
		/// <param name="eventType">
		///     Specifies the type of event.
		///     The event type is used to determine which event handler to invoke.
		///     For a list of event types, see <see cref="WalletEventManager" />.
		///     <example>
		///     </example>
		/// </param>
		/// <param name="data">
		///     Contains the data associated with the event in a JSON string format,
		///     which is further parsed in specific event handlers.
		/// </param>
		/// <example>
		///     The data for a <see cref="WalletEventManager.EventTypeWalletConnected" /> event
		///     is a <see cref="WalletInfo" /> object serialized into a JSON string.
		/// </example>
		public UnifiedEvent(string eventType, string data = null)
		{
			data ??= "{}";

			EventType = eventType;
			Data = data;
		}

		public string GetData()
		{
			return Data;
		}

		public string GetEventType()
		{
			return EventType;
		}
	}

}