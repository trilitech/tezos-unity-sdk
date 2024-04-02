using System;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Beacon;
using TezosSDK.Helpers;
using TezosSDK.Helpers.Extensions;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos.Wallet
{

	public class WalletProvider : IWalletProvider, IDisposable
	{
		private readonly IBeaconConnector _beaconConnector;
		private string _pubKey;
		private string _signature;
		private string _transactionHash;

		public WalletProvider(IWalletEventManager eventManager, IBeaconConnector beaconConnector)
		{
			EventManager = eventManager;
			EventManager.HandshakeReceived += OnHandshakeReceived;
			EventManager.WalletConnected += OnWalletConnected;
			EventManager.WalletDisconnected += OnWalletDisconnected;
			EventManager.PayloadSigned += OnPayloadSigned;
			EventManager.OperationInjected += OnOperationInjected;

			_beaconConnector = beaconConnector;
			_beaconConnector.OperationRequested += OnOperationRequested;
		}

		public void Dispose()
		{
			if (_beaconConnector is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		public bool IsConnected { get; private set; }
		public HandshakeData HandshakeData { get; private set; }

		public IWalletEventManager EventManager { get; }

		public void Connect(WalletProviderType walletProvider)
		{
			_beaconConnector.ConnectWallet(walletProvider);
		}

		public void Disconnect()
		{
			_beaconConnector.DisconnectWallet();
		}

		public string GetWalletAddress()
		{
			return _beaconConnector.GetWalletAddress();
		}

		public void RequestSignPayload(SignPayloadType signingType, string payload)
		{
			_beaconConnector.RequestSignPayload(signingType, payload);
		}

		public bool VerifySignedPayload(SignPayloadType signingType, string payload)
		{
			return NetezosExtensions.VerifySignature(_pubKey, signingType, payload, _signature);
		}

		public void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0)
		{
			_beaconConnector.RequestOperation(contractAddress, entryPoint, input, amount);
		}

		public void OriginateContract(string script, string delegateAddress)
		{
			_beaconConnector.RequestContractOrigination(script, delegateAddress);
		}

		/// <summary>
		///     Raised when an operation requiring user interaction is requested by the IBeaconConnector implementation.
		/// </summary>
		private void OnOperationRequested(BeaconMessageType beaconMessageType)
		{
			Logger.LogDebug($"WalletProvider.OnOperationRequested of type: {beaconMessageType}");

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			// The wallet will already be open for the pairing request during login
			// We should ignore this message type
			if (beaconMessageType != BeaconMessageType.permission_request)
			{
				OpenWallet();
			}
#endif
		}

		private void OpenWallet()
		{
			Logger.LogDebug("WalletProvider.OpenWallet");

			// OpenURL can only be called from the main thread.
			UnityMainThreadDispatcher.Enqueue(() => { Application.OpenURL("tezos://"); });
		}

		private void OnWalletDisconnected(WalletInfo obj)
		{
			IsConnected = false;
			HandshakeData = null;
		}

		/// <summary>
		///     An operation has been injected into the network (i.e. the transaction has been sent to the network).
		///     Raised when an operation is injected into the network and the operation hash is received. (By the IBeaconConnector
		///     implementation)
		/// </summary>
		/// <param name="transaction"></param>
		private void OnOperationInjected(OperationInfo transaction)
		{
			StartTrackingOperation(transaction);
		}

		private void StartTrackingOperation(OperationInfo transaction)
		{
			var operationHash = transaction.TransactionHash;
			var tracker = new OperationTracker(operationHash, OnComplete);
			tracker.BeginTracking();
			return;

			// Local callback function that is invoked when the operation tracking is complete.
			void OnComplete(bool isSuccess, string errorMessage)
			{
				if (isSuccess)
				{
					var operationResult = new OperationInfo(operationHash, transaction.Id, transaction.OperationType);

					var completedEvent = new UnifiedEvent(WalletEventManager.EventTypeOperationCompleted,
						JsonUtility.ToJson(operationResult));

					Logger.LogDebug($"Operation completed: {operationHash}");

					EventManager.HandleEvent(completedEvent);
				}
				else
				{
					Logger.LogError($"Operation failed: {errorMessage}");

					var errorinfo = new OperationInfo(operationHash, transaction.Id, transaction.OperationType,
						errorMessage);

					var failEvent = new UnifiedEvent(WalletEventManager.EventTypeOperationFailed,
						JsonUtility.ToJson(errorinfo));

					EventManager.HandleEvent(failEvent);
				}
			}
		}

		private void OnPayloadSigned(SignResult payload)
		{
			_signature = payload.Signature;
		}

		private void OnWalletConnected(WalletInfo wallet)
		{
			_pubKey = wallet.PublicKey;
			IsConnected = true;
		}

		private void OnHandshakeReceived(HandshakeData handshake)
		{
			if (HandshakeData != null)
			{
				return;
			}

			Logger.LogDebug("WalletProvider.OnHandshakeReceived");

			HandshakeData = handshake;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			PairWithWallet();
#endif
		}

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		private void PairWithWallet()
		{
			UnityMainThreadDispatcher.Enqueue(() =>
			{
				Logger.LogDebug("WalletProvider.PairWithWallet (OpenURL)");
				Application.OpenURL($"tezos://?type=tzip10&data={HandshakeData.PairingData}");
			});
		}
#endif
	}

}