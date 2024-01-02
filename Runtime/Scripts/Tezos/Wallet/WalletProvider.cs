#region

using System;
using System.Collections;
using System.Text.Json;
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
			EventManager.PairingCompleted += OnPairingCompleted;
			_beaconConnector = beaconConnector;
		}

		private void OnWalletDisconnected(WalletInfo obj)
		{
			IsConnected = false;
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
			_beaconConnector.OnReady();
		}

		public void Connect(WalletProviderType walletProvider, bool withRedirectToWallet = false)
		{
			// _beaconConnector.InitWalletProvider(
			// 	network: TezosConfig.Instance.Network.ToString(),
			// 	rpc: TezosConfig.Instance.RpcBaseUrl,
			// 	walletProviderType: walletProvider,
			// 	metadata: _dAppMetadata);
			
			_beaconConnector.ConnectAccount();
		}

		public void Disconnect()
		{
			_beaconConnector.DisconnectAccount();
		}

		public string GetActiveAddress()
		{
			return _beaconConnector.GetActiveAccountAddress();
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

		private void OnPairingCompleted(PairingDoneData _)
		{
			_beaconConnector.RequestTezosPermission(TezosConfig.Instance.Network.ToString());
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
			HandshakeData = handshake;
		}
		
		public void RequestTezosPermission()
		{
			_beaconConnector.RequestTezosPermission(TezosConfig.Instance.Network.ToString());
		}

#if UNITY_ANDROID || UNITY_IOS
		public void OpenWallet()
		{
			Application.OpenURL($"tezos://?type=tzip10&data={HandshakeData.PairingData}");
		}
#endif

		// TODO: Find a better place for this, used to be in WalletMessageReceiver
		private IEnumerator TrackTransaction(string transactionHash)
		{
			var success = false;
			const float _timeout = 30f; // seconds
			var timestamp = Time.time;

			// keep making requests until time out or success
			while (!success && Time.time - timestamp < _timeout)
			{
				Logger.LogDebug($"Checking transaction status: {transactionHash}");

				yield return TezosManager.Instance.Tezos.API.GetOperationStatus(OnStatusResponse, transactionHash);

				yield return new WaitForSecondsRealtime(3);
				continue;

				void OnStatusResponse(bool? result) // local callback function
				{
					if (result != null)
					{
						success = JsonSerializer.Deserialize<bool>(result);
					}
				}
			}

			var operationResult = new OperationResult
			{
				TransactionHash = transactionHash
			};

			var contractCallCompletedEvent = new UnifiedEvent(WalletEventManager.EventTypeContractCallCompleted,
				JsonUtility.ToJson(operationResult));

			EventManager.HandleEvent(contractCallCompletedEvent);
		}
	}

}