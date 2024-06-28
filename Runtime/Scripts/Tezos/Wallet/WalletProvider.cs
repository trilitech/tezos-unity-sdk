﻿using System;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Helpers;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.WalletServices.Connectors;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using TezosSDK.WalletServices.Interfaces;
using UnityEngine;

namespace TezosSDK.Tezos.Wallet
{

	public class WalletProvider : IWalletConnection, IWalletAccount, IWalletTransaction, IWalletContract,
		IWalletEventProvider, IDisposable
	{
		private readonly IWalletConnector _walletConnector;
		private string _pubKey;
		private string _signature;
		private string _transactionHash;

		public WalletProvider(IWalletEventManager eventManager, IWalletConnector walletConnector)
		{
			EventManager = eventManager;
			EventManager.HandshakeReceived += OnHandshakeReceived;
			EventManager.WalletConnected += OnWalletConnected;
			EventManager.WalletDisconnected += OnWalletDisconnected;
			EventManager.PayloadSigned += OnPayloadSigned;
			EventManager.OperationInjected += OnOperationInjected;
			_walletConnector = walletConnector;
			_walletConnector.OperationRequested += OperationRequestedHandler;
		}

		public IWalletEventManager EventManager { get; }

		public void Dispose()
		{
			if (_walletConnector is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		public string GetWalletAddress()
		{
			return _walletConnector.GetWalletAddress();
		}

		public bool IsConnected { get; private set; }
		public HandshakeData HandshakeData { get; private set; }

		public void Connect()
		{
			_walletConnector.ConnectWallet();
		}

		public void Disconnect()
		{
			_walletConnector.DisconnectWallet();
		}

		public void OriginateContract(string script, string delegateAddress)
		{
			TezosLogger.LogDebug($"WalletProvider.OriginateContract (ConnectorType: {_walletConnector.ConnectorType})");

			var originationRequest = new WalletOriginateContractRequest
			{
				Script = script,
				DelegateAddress = delegateAddress
			};

			_walletConnector.RequestContractOrigination(originationRequest);
		}

		public void RequestSignPayload(SignPayloadType signingType, string payload)
		{
			var signRequest = new WalletSignPayloadRequest
			{
				SigningType = signingType,
				Payload = payload
			};

			_walletConnector.RequestSignPayload(signRequest);
		}

		public bool VerifySignedPayload(SignPayloadType signingType, string payload)
		{
			return NetezosExtensions.VerifySignature(_pubKey, signingType, payload, _signature);
		}

		public void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0)
		{
			var operationRequest = new WalletOperationRequest
			{
				Destination = contractAddress,
				EntryPoint = entryPoint,
				Arg = input,
				Amount = amount
			};

			_walletConnector.RequestOperation(operationRequest);
		}

		/// <summary>
		///     Raised when an operation requiring user interaction is requested by the IBeaconConnector implementation.
		/// </summary>
		private void OperationRequestedHandler(WalletMessageType messageType)
		{
			TezosLogger.LogDebug($"WalletProvider.OperationRequestedHandler messageType: {messageType}");

			switch (messageType)
			{
				case WalletMessageType.ConnectionRequest:
					OpenWallet();
					break;
				case WalletMessageType.OperationRequest:
					OpenWallet();
					break;
				case WalletMessageType.SignPayloadRequest:
					break;
			}
		}

		private void OpenWallet()
		{
			TezosLogger.LogDebug("WalletProvider.OpenWallet");

			string url = "tezos://";
			
			// OpenURL can only be called from the main thread.
			UnityMainThreadDispatcher.Enqueue(() => { Application.OpenURL(url); });
		}

		private void OnWalletDisconnected(WalletInfo obj)
		{
			IsConnected = false;
			HandshakeData = null;
		}

		/// <summary>
		///     An operation has been injected into the network (i.e. the transaction has been sent to the network).
		///     Raised when an operation is injected into the network and the operation hash is received.
		/// </summary>
		/// <param name="transaction"></param>
		private void OnOperationInjected(OperationInfo transaction)
		{
			TezosLogger.LogDebug($"WalletProvider: Handling OperationInjected event. Hash: {transaction.Hash}"); 
			StartTrackingOperation(transaction);
		}

		private void StartTrackingOperation(OperationInfo transaction)
		{
			TezosLogger.LogDebug($"WalletProvider.StartTrackingOperation: {transaction.Hash}");
			
			var operationHash = transaction.Hash;
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

					TezosLogger.LogDebug($"Operation completed: {operationHash}");

					EventManager.HandleEvent(completedEvent);
				}
				else
				{
					TezosLogger.LogError($"Operation failed: {errorMessage}");

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

			TezosLogger.LogDebug("WalletProvider.OnHandshakeReceived");

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
				TezosLogger.LogDebug("WalletProvider.PairWithWallet (OpenURL)");
				Application.OpenURL($"tezos://?type=tzip10&data={HandshakeData.PairingData}");
			});
		}
#endif
	}

}