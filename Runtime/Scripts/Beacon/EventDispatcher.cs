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
			var accountInfo = CreateWalletInfo(activeWallet);

			return new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeAccountDisconnected,
				Data = JsonUtility.ToJson(accountInfo)
			};
		}

		private void DispatchEvent(UnifiedEvent eventData)
		{
			var json = JsonUtility.ToJson(eventData);
			UnityMainThreadDispatcher.Enqueue(() => _eventManager.HandleEvent(json));
		}

		public void DispatchAccountConnectedEvent(DappBeaconClient beaconDappClient)
		{
			var accountConnectedEvent = CreateAccountConnectedEvent(beaconDappClient.GetActiveAccount());
			DispatchEvent(accountConnectedEvent);
		}

		private UnifiedEvent CreateAccountConnectedEvent(PermissionInfo activeAccountPermissions)
		{
			var accountInfo = CreateWalletInfo(activeAccountPermissions);

			return new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeAccountConnected,
				Data = JsonUtility.ToJson(accountInfo)
			};
		}

		private WalletInfo CreateWalletInfo(PermissionInfo activeAccountPermissions)
		{
			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			return new WalletInfo
			{
				Address = pubKey.Address,
				PublicKey = activeAccountPermissions.PublicKey
			};
		}

		public void DispathPairingDoneEvent(DappBeaconClient beaconDappClient)
		{
			var pairingDoneData = new PairingDoneData
			{
				DAppPublicKey = beaconDappClient.GetActiveAccount()?.PublicKey,
				Timestamp = DateTime.UtcNow.ToString("o")
			};

			var pairingDoneEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypePairingDone,
				Data = JsonUtility.ToJson(pairingDoneData)
			};

			DispatchEvent(pairingDoneEvent);
		}

		public void DispatchContractCallInjectedEvent(OperationResponse operationResponse)
		{
			var operationResult = new OperationResult
			{
				TransactionHash = operationResponse.TransactionHash
			};

			var contractEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeContractCallInjected,
				Data = JsonUtility.ToJson(operationResult)
			};

			DispatchEvent(contractEvent);
		}

		public void DispatchPayloadSignedEvent(SignPayloadResponse signPayloadResponse)
		{
			var signResult = new SignResult
			{
				Signature = signPayloadResponse.Signature
			};

			var signedEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypePayloadSigned,
				Data = JsonUtility.ToJson(signResult)
			};

			DispatchEvent(signedEvent);
		}

		public void DispatchHandshakeEvent(DappBeaconClient beaconDappClient)
		{
			var handshakeData = new HandshakeData
			{
				PairingData = beaconDappClient.GetPairingRequestInfo()
			};

			var handshakeEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeHandshakeReceived,
				Data = JsonUtility.ToJson(handshakeData)
			};

			DispatchEvent(handshakeEvent);
		}
	}

}