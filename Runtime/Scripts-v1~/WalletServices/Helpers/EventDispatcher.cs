using System;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using Newtonsoft.Json;
using TezosSDK.Helpers;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Interfaces;

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
		private readonly IWalletEventManager _eventManager;

		public EventDispatcher(IWalletEventManager eventManager)
		{
			_eventManager = eventManager;
		}

		public void DispatchWalletDisconnectedEvent(WalletInfo disconnectedWallet)
		{
			TezosLogger.LogDebug($"Dispatching WalletDisconnectedEvent for {disconnectedWallet?.PublicKey}");

			var walletDisconnectedEvent = new UnifiedEvent(WalletEventManager.EventTypeWalletDisconnected, JsonConvert.SerializeObject(disconnectedWallet));
			DispatchEvent(walletDisconnectedEvent);
		}

		public void DispatchOperationFailedEvent(OperationInfo operationResponse)
		{
			var eventType = WalletEventManager.EventTypeOperationFailed;
			var data = JsonConvert.SerializeObject(operationResponse);

			var unifiedEvent = new UnifiedEvent(eventType, data);
			DispatchEvent(unifiedEvent);
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
			return new UnifiedEvent(WalletEventManager.EventTypeWalletConnected, JsonConvert.SerializeObject(walletInfo));
		}

		public void DispatchPairingCompletedEvent(DappBeaconClient beaconDappClient)
		{
			TezosLogger.LogDebug("Dispatching PairingCompletedEvent");

			var pairingDoneData = new PairingDoneData
			{
				DAppPublicKey = beaconDappClient.GetActiveAccount()?.PublicKey,
				Timestamp = DateTime.UtcNow.ToString("o")
			};

			var pairingDoneEvent = new UnifiedEvent(WalletEventManager.EventTypePairingDone, JsonConvert.SerializeObject(pairingDoneData));
			DispatchEvent(pairingDoneEvent);
		}

		public void DispatchOperationInjectedEvent(OperationInfo operationResponse)
		{
			var eventType = WalletEventManager.EventTypeOperationInjected;
			var data = JsonConvert.SerializeObject(operationResponse);

			var unifiedEvent = new UnifiedEvent(eventType, data);
			DispatchEvent(unifiedEvent);
		}

		public void DispatchPayloadSignedEvent(SignPayloadResponse signPayloadResponse)
		{
			var signResult = new SignResult
			{
				Signature = signPayloadResponse.Signature
			};

			var signedEvent = new UnifiedEvent(WalletEventManager.EventTypePayloadSigned, JsonConvert.SerializeObject(signResult));
			DispatchEvent(signedEvent);
		}

		public void DispatchPayloadSignedEvent(SignResult signResult)
		{
			var signedEvent = new UnifiedEvent(WalletEventManager.EventTypePayloadSigned, JsonConvert.SerializeObject(signResult));
			DispatchEvent(signedEvent);
		}

		public void DispatchPairingRequestEvent(string pairingData)
		{
			TezosLogger.LogDebug("Dispatching PairingRequestEvent");

			var handshakeData = new PairingRequestData
			{
				Data = pairingData
			};

			var pairingReqEvent = new UnifiedEvent(WalletEventManager.EventTypePairingRequest, JsonConvert.SerializeObject(handshakeData));
			DispatchEvent(pairingReqEvent);
		}
	}

}