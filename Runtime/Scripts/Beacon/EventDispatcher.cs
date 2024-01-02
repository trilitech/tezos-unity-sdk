#region

using System;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.Core.Domain.Entities;
using Netezos.Keys;
using TezosSDK.Helpers;
using UnityEngine;

#endregion

namespace TezosSDK.Beacon
{

	public class EventDispatcher
	{
		private readonly WalletEventManager _eventManager;

		public EventDispatcher(WalletEventManager eventManager)
		{
			_eventManager = eventManager;
		}

		public void DispatchWalletDisconnectedEvent(PermissionInfo activeWallet)
		{
			var walletDisconnectedEvent = CreateWalletDisconnectedEvent(activeWallet);
			DispatchEvent(walletDisconnectedEvent);
		}

		private UnifiedEvent CreateWalletDisconnectedEvent(PermissionInfo activeWallet)
		{
			WalletInfo walletInfo = CreateWalletInfo(activeWallet);

			return new UnifiedEvent(WalletEventManager.EventTypeAccountDisconnected, JsonUtility.ToJson(walletInfo));
		}

		/// <summary>
		/// Dispatches an event to the Unity main thread.
		/// </summary>
		/// <param name="eventData"></param>
		private void DispatchEvent(UnifiedEvent eventData)
		{
			UnityMainThreadDispatcher.Enqueue(() => _eventManager.HandleEvent(eventData));
		}

		public void DispatchAccountConnectedEvent(DappBeaconClient beaconDappClient)
		{
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

		public void DispatchPairingDoneEvent(DappBeaconClient beaconDappClient)
		{
			var pairingDoneData = new PairingDoneData
			{
				DAppPublicKey = beaconDappClient.GetActiveAccount()?.PublicKey,
				Timestamp = DateTime.UtcNow.ToString("o")
			};

			var pairingDoneEvent = new UnifiedEvent(WalletEventManager.EventTypePairingDone, JsonUtility.ToJson(pairingDoneData));

			DispatchEvent(pairingDoneEvent);
		}

		public void DispatchContractCallInjectedEvent(OperationResponse operationResponse)
		{
			var operationResult = new OperationResult
			{
				TransactionHash = operationResponse.TransactionHash
			};

			var contractEvent = new UnifiedEvent(WalletEventManager.EventTypeContractCallInjected, JsonUtility.ToJson(operationResult));

			DispatchEvent(contractEvent);
		}

		public void DispatchPayloadSignedEvent(SignPayloadResponse signPayloadResponse)
		{
			var signResult = new SignResult
			{
				Signature = signPayloadResponse.Signature
			};

			var signedEvent = new UnifiedEvent(WalletEventManager.EventTypePayloadSigned, JsonUtility.ToJson(signResult));

			DispatchEvent(signedEvent);
		}

		public void DispatchHandshakeEvent(string pairingData)
		{
			var handshakeData = new HandshakeData
			{
				PairingData = pairingData
			};

			var handshakeEvent = new UnifiedEvent(WalletEventManager.EventTypeHandshakeReceived, JsonUtility.ToJson(handshakeData));
			DispatchEvent(handshakeEvent);
		}
	}

}