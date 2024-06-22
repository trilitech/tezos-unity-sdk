using System;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using TezosSDK.Helpers;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using UnityEngine;

namespace TezosSDK.WalletServices.Helpers
{

	/// <summary>
	///     A helper class to dispatch events to the Unity main thread.
	///     This is necessary because the Beacon SDK is running on
	///     a background thread, and when we receive events from it, we need to dispatch them or deal with them on the main
	///     thread.
	/// </summary>
	public class EventDispatcher
	{
		private readonly WalletEventManager _eventManager;

		public EventDispatcher(WalletEventManager eventManager)
		{
			_eventManager = eventManager;
		}

		public void DispatchWalletDisconnectedEvent(WalletInfo disconnectedWallet)
		{
			TezosLogger.LogDebug($"Dispatching WalletDisconnectedEvent for {disconnectedWallet?.PublicKey}");

			var walletDisconnectedEvent = new UnifiedEvent(WalletEventManager.EventTypeWalletDisconnected,
				JsonUtility.ToJson(disconnectedWallet));

			DispatchEvent(walletDisconnectedEvent);
		}

		/// <summary>
		///     Dispatches an event to the Unity main thread.
		/// </summary>
		/// <param name="eventData"></param>
		private void DispatchEvent(UnifiedEvent eventData)
		{
			TezosLogger.LogDebug($"Dispatching event: {eventData.GetEventType()}");
			UnityMainThreadDispatcher.Enqueue(() => _eventManager.HandleEvent(eventData));
		}

		public void DispatchWalletConnectedEvent(WalletInfo walletInfo)
		{
			var walletConnectedEvent = CreateWalletConnectedEvent(walletInfo);
			DispatchEvent(walletConnectedEvent);
		}

		private UnifiedEvent CreateWalletConnectedEvent(WalletInfo walletInfo)
		{
			return new UnifiedEvent(WalletEventManager.EventTypeWalletConnected, JsonUtility.ToJson(walletInfo));
		}

		public void DispatchPairingCompletedEvent(DappBeaconClient beaconDappClient)
		{
			TezosLogger.LogDebug("Dispatching PairingCompletedEvent");

			var pairingDoneData = new PairingDoneData
			{
				DAppPublicKey = beaconDappClient.GetActiveAccount()?.PublicKey,
				Timestamp = DateTime.UtcNow.ToString("o")
			};

			var pairingDoneEvent = new UnifiedEvent(WalletEventManager.EventTypePairingDone,
				JsonUtility.ToJson(pairingDoneData));

			DispatchEvent(pairingDoneEvent);
		}

		public void DispatchOperationInjectedEvent(OperationInfo operationResponse)
		{
			var unifiedEvent = new UnifiedEvent(WalletEventManager.EventTypeOperationInjected,
				JsonUtility.ToJson(operationResponse));

			DispatchEvent(unifiedEvent);
		}

		public void DispatchPayloadSignedEvent(SignPayloadResponse signPayloadResponse)
		{
			var signResult = new SignResult
			{
				Signature = signPayloadResponse.Signature
			};

			var signedEvent =
				new UnifiedEvent(WalletEventManager.EventTypePayloadSigned, JsonUtility.ToJson(signResult));

			DispatchEvent(signedEvent);
		}

		public void DispatchHandshakeEvent(string pairingData)
		{
			TezosLogger.LogDebug("Dispatching HandshakeEvent");

			var handshakeData = new HandshakeData
			{
				PairingData = pairingData
			};

			var handshakeEvent = new UnifiedEvent(WalletEventManager.EventTypeHandshakeReceived,
				JsonUtility.ToJson(handshakeData));

			DispatchEvent(handshakeEvent);
		}
	}

}