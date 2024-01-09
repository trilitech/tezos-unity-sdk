using System;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.Core.Domain.Entities;
using Netezos.Keys;
using TezosSDK.Helpers;
using TezosSDK.Tezos;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Beacon
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

		public void DispatchWalletDisconnectedEvent(WalletInfo activeWallet)
		{
			Logger.Log($"Dispatching WalletDisconnectedEvent for {activeWallet?.PublicKey}");

			var walletDisconnectedEvent = new UnifiedEvent(WalletEventManager.EventTypeWalletDisconnected,
				JsonUtility.ToJson(activeWallet));

			DispatchEvent(walletDisconnectedEvent);
		}

		/// <summary>
		///     Dispatches an event to the Unity main thread.
		/// </summary>
		/// <param name="eventData"></param>
		private void DispatchEvent(UnifiedEvent eventData)
		{
			Logger.Log("Dispatching event");
			UnityMainThreadDispatcher.Enqueue(() => _eventManager.HandleEvent(eventData));
		}

		public void DispatchWalletConnectedEvent(DappBeaconClient beaconDappClient)
		{
			Logger.Log("Dispatching WalletConnectedEvent");
			var accountConnectedEvent = CreateWalletConnectedEvent(beaconDappClient.GetActiveAccount());
			DispatchEvent(accountConnectedEvent);
		}

		private UnifiedEvent CreateWalletConnectedEvent(PermissionInfo activeAccountPermissions)
		{
			var walletInfo = CreateWalletInfo(activeAccountPermissions);

			return new UnifiedEvent(WalletEventManager.EventTypeWalletConnected, JsonUtility.ToJson(walletInfo));
		}

		private WalletInfo CreateWalletInfo(PermissionInfo activeWalletPermissions)
		{
			var pubKey = PubKey.FromBase58(activeWalletPermissions.PublicKey);

			return new WalletInfo
			{
				Address = pubKey.Address,
				PublicKey = activeWalletPermissions.PublicKey
			};
		}

		public void DispatchPairingCompletedEvent(DappBeaconClient beaconDappClient)
		{
			Logger.Log("Dispatching PairingCompletedEvent");

			var pairingDoneData = new PairingDoneData
			{
				DAppPublicKey = beaconDappClient.GetActiveAccount()?.PublicKey,
				Timestamp = DateTime.UtcNow.ToString("o")
			};

			var pairingDoneEvent = new UnifiedEvent(WalletEventManager.EventTypePairingDone,
				JsonUtility.ToJson(pairingDoneData));

			DispatchEvent(pairingDoneEvent);
		}

		public void DispatchContractCallInjectedEvent(OperationResponse operationResponse)
		{
			var operationResult = new OperationResult
			{
				TransactionHash = operationResponse.TransactionHash
			};

			var contractEvent = new UnifiedEvent(WalletEventManager.EventTypeContractCallInjected,
				JsonUtility.ToJson(operationResult));

			DispatchEvent(contractEvent);
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
			Logger.Log("Dispatching HandshakeEvent");

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