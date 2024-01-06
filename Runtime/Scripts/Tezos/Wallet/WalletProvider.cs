#region

using System;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Beacon;
using TezosSDK.Helpers;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

#endregion

namespace TezosSDK.Tezos.Wallet
{

	public class WalletProvider : IWalletProvider, IDisposable
	{
		private readonly IBeaconConnector _beaconConnector;
		private string _pubKey;
		private string _signature;
		private string _transactionHash;

		public WalletProvider(WalletEventManager eventManager, IBeaconConnector beaconConnector)
		{
			EventManager = eventManager;
			EventManager.HandshakeReceived += OnHandshakeReceived;
			EventManager.WalletConnected += OnWalletConnected;
			EventManager.WalletDisconnected += OnWalletDisconnected;
			EventManager.PayloadSigned += OnPayloadSigned;
			EventManager.ContractCallInjected += OnContractCallInjected;
			_beaconConnector = beaconConnector;
		}

		private void OnWalletDisconnected(WalletInfo obj)
		{
			IsConnected = false;
			HandshakeData = null;
		}

		public bool IsConnected { get; private set; }
		public HandshakeData HandshakeData { get; private set; }

		public void Dispose()
		{
			if (_beaconConnector is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		public WalletEventManager EventManager { get; }

		public void OnReady()
		{
			Logger.LogDebug("WalletProvider.OnReady");
			_beaconConnector.OnReady();
		}

		public void Connect(WalletProviderType walletProvider)
		{
#if UNITY_WEBGL
			_beaconConnector.InitWalletProvider("", "", walletProvider, null);
#endif
			_beaconConnector.ConnectWallet();
		}

		public void Disconnect()
		{
			_beaconConnector.DisconnectWallet();
		}

		public string GetActiveAddress()
		{
			return _beaconConnector.GetActiveWalletAddress();
		}

		public void RequestSignPayload(SignPayloadType signingType, string payload)
		{
			_beaconConnector.RequestTezosSignPayload(signingType, payload);
		}

		public bool VerifySignedPayload(SignPayloadType signingType, string payload)
		{
			return NetezosExtensions.VerifySignature(_pubKey, signingType, payload, _signature);
		}

		public void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0)
		{
			_beaconConnector.RequestTezosOperation(contractAddress, entryPoint, input, amount);
		}

		public void OriginateContract(string script, string delegateAddress)
		{
			_beaconConnector.RequestContractOrigination(script, delegateAddress);
		}

		private void OnContractCallInjected(OperationResult transaction)
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
					var operationResult = new OperationResult
					{
						TransactionHash = operationHash
					};

					var contractCallCompletedEvent = new UnifiedEvent(WalletEventManager.EventTypeContractCallCompleted,
						JsonUtility.ToJson(operationResult));

					Logger.LogDebug($"Contract call completed: {operationHash}");
					EventManager.HandleEvent(contractCallCompletedEvent);
				}
				else
				{
					Logger.LogError($"Contract call failed: {errorMessage}");

					var errorinfo = new ErrorInfo
					{
						Message = errorMessage
					};

					var contractCallFailedEvent = new UnifiedEvent(WalletEventManager.EventTypeContractCallFailed,
						JsonUtility.ToJson(errorinfo));

					EventManager.HandleEvent(contractCallFailedEvent);
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

		private void PairWithWallet()
		{
			Logger.LogDebug("WalletProvider.PairWithWallet");
			Application.OpenURL($"tezos://?type=tzip10&data={HandshakeData.PairingData}");
		}
	}

}