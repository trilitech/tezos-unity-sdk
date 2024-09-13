using Beacon.Sdk.Beacon.Sign;
using Newtonsoft.Json;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Interfaces;
using UnityEngine;
// ReSharper disable once RedundantUsingDirective
using TezosSDK.Helpers;
// using TezosSDK.Tezos.Managers;
using TezosSDK.WalletServices.Connectors;

namespace TezosSDK.Tezos.Wallet
{

	public class WalletProvider : IWalletConnection, IWalletAccount, IWalletTransaction, IWalletContract, IWalletEventProvider
	{
		private const string _WALLET_INFO_KEY = "wallet-info-key";
		
		private string _pubKey;
		private string _signature;
		private string _transactionHash;
		
		public WalletInfo WalletInfo { get; private set; }

		public WalletProvider(IWalletEventManager eventManager)
		{
			EventManager = eventManager;
			EventManager.WalletConnected += OnWalletConnected;
			EventManager.WalletDisconnected += OnWalletDisconnected;
			EventManager.PayloadSigned += OnPayloadSigned;
			EventManager.OperationInjected += OnOperationInjected;

			WalletInfo = JsonConvert.DeserializeObject<WalletInfo>(PlayerPrefs.GetString(_WALLET_INFO_KEY, null));
		}

		public bool               IsConnected        => WalletInfo != null;
		public PairingRequestData PairingRequestData => WalletInfo != null ? WalletConnectorFactory.GetConnector(WalletInfo.ConnectorType).PairingRequestData : null;

		public void Connect(ConnectorType connectorType)
		{
			if (IsConnected)
			{
				TezosLogger.LogWarning("Wallet is already connected, use GetWalletAddress() to retrieve the address");
				OnWalletConnected(WalletInfo);
				return;
			}
			WalletConnectorFactory.GetConnector(connectorType).ConnectWallet();
		}

		public string GetWalletAddress()
		{
			if (!IsConnected)
			{
				TezosLogger.LogWarning("Wallet is NOT connected.");
				return null;
			}

			return WalletInfo.Address;
		}
		public void Disconnect()                                                                  => WalletConnectorFactory.GetConnector(WalletInfo.ConnectorType).DisconnectWallet();
		public void RequestOperation(WalletOperationRequest                   operationRequest)   => WalletConnectorFactory.GetConnector(WalletInfo.ConnectorType).RequestOperation(operationRequest);
		public void RequestSignPayload(WalletSignPayloadRequest               signRequest)        => WalletConnectorFactory.GetConnector(WalletInfo.ConnectorType).RequestSignPayload(signRequest);
		public void RequestContractOrigination(WalletOriginateContractRequest originationRequest) => WalletConnectorFactory.GetConnector(WalletInfo.ConnectorType).RequestContractOrigination(originationRequest);

		public void OriginateContract(string script, string delegateAddress)
		{
			TezosLogger.LogDebug($"WalletProvider.OriginateContract (ConnectorType: {WalletInfo.ConnectorType})");

			var originationRequest = new WalletOriginateContractRequest
			{
				Script = script,
				DelegateAddress = delegateAddress
			};

			WalletConnectorFactory.GetConnector(WalletInfo.ConnectorType).RequestContractOrigination(originationRequest);
		}

		public IWalletEventManager EventManager { get; }

		public void RequestSignPayload(SignPayloadType signingType, string payload)
		{
			var signRequest = new WalletSignPayloadRequest
			{
				SigningType = signingType,
				Payload = payload
			};

			WalletConnectorFactory.GetConnector(WalletInfo.ConnectorType).RequestSignPayload(signRequest);
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

			WalletConnectorFactory.GetConnector(WalletInfo.ConnectorType).RequestOperation(operationRequest);
		}

		private void OnWalletDisconnected(WalletInfo _)
		{
			WalletInfo = null;
			PlayerPrefs.SetString(_WALLET_INFO_KEY, JsonConvert.SerializeObject(WalletInfo));
		}

		/// <summary>
		///     An operation has been injected into the network (i.e. the transaction has been sent to the network).
		///     Raised when an operation is injected into the network and the operation hash is received.
		/// </summary>
		private void OnOperationInjected(OperationInfo operation)
		{
			TezosLogger.LogDebug($"WalletProvider: Handling OperationInjected event. Hash: {operation.Hash}");
			StartTrackingOperation(operation);
		}

		private void StartTrackingOperation(OperationInfo operation)
		{
			TezosLogger.LogDebug($"WalletProvider.StartTrackingOperation: {operation.Hash}");

			var operationHash = operation.Hash;
			var tracker = new OperationTracker(operationHash, OnComplete);
			tracker.BeginTracking();
			return;

			// Local callback function that is invoked when the operation tracking is complete.
			void OnComplete(bool isSuccess, string errorMessage)
			{
				if (isSuccess)
				{
					var operationResult = new OperationInfo(operationHash, operation.Id, operation.OperationType);

					var completedEvent = new UnifiedEvent(WalletEventManager.EventTypeOperationCompleted, JsonUtility.ToJson(operationResult));

					TezosLogger.LogDebug($"Operation completed: {operationHash}");

					EventManager.HandleEvent(completedEvent);
				}
				else
				{
					TezosLogger.LogError($"Operation failed: {errorMessage}");

					var errorinfo = new OperationInfo(operationHash, operation.Id, operation.OperationType, errorMessage);

					var failEvent = new UnifiedEvent(WalletEventManager.EventTypeOperationFailed, JsonUtility.ToJson(errorinfo));

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
			WalletInfo = wallet;
			PlayerPrefs.SetString(_WALLET_INFO_KEY, JsonConvert.SerializeObject(wallet));
		}

// 		private void OnHandshakeReceived(PairingRequestData pairingRequest)
// 		{
// 			if (PairingRequestData != null)
// 			{
// 				return;
// 			}
//
// 			TezosLogger.LogDebug("WalletProvider.OnHandshakeReceived");
//
// 			PairingRequestData = pairingRequest;
//
// #if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
// 			PairWithWallet();
// #endif
// 		}

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		private void PairWithWallet(string data)
		{
			UnityMainThreadDispatcher.Enqueue(() =>
			{
				TezosLogger.LogDebug("WalletProvider.PairWithWallet (OpenURL)");
				Application.OpenURL($"tezos://?type=tzip10&data={data}");
			});
		}
#endif
	}
}