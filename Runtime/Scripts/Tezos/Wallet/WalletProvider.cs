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
		private readonly DAppMetadata _dAppMetadata;
		private IBeaconConnector _beaconConnector;
		private string _handshake;
		private string _pubKey;
		private string _signature;
		private string _transactionHash;
		private readonly bool _withRedirectToWallet;

		public WalletProvider(DAppMetadata dAppMetadata, bool withRedirectToWallet = false)
		{
			_dAppMetadata = dAppMetadata;
			_withRedirectToWallet = withRedirectToWallet;

			// Create or get a WalletMessageReceiver Game object to receive callback messages
			var eventManager = GameObject.Find("WalletEventManager");

			EventManager = eventManager != null
				? eventManager.GetComponent<WalletEventManager>()
				: new GameObject("WalletEventManager").AddComponent<WalletEventManager>();

			EventManager.HandshakeReceived += OnHandshakeReceived;

			InitBeaconConnector();
		}

		public bool IsConnected { get; private set; }

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

		public void Connect(WalletProviderType walletProvider, bool withRedirectToWallet)
		{
			_beaconConnector.InitWalletProvider(
				network: TezosConfig.Instance.Network.ToString(),
				rpc: TezosConfig.Instance.RpcBaseUrl,
				walletProviderType: walletProvider,
				metadata: _dAppMetadata);
			
			_beaconConnector.ConnectAccount();
			IsConnected = true;
		}

		public void Disconnect()
		{
			_beaconConnector.DisconnectAccount();
			IsConnected = false;
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
			_beaconConnector.RequestTezosOperation(contractAddress, entryPoint, input, amount,
				TezosConfig.Instance.Network.ToString(), TezosConfig.Instance.RpcBaseUrl);
		}

		public void OriginateContract(string script, string delegateAddress)
		{
			_beaconConnector.RequestContractOrigination(script, delegateAddress);
		}

		private void InitBeaconConnector()
		{
			// Assign the BeaconConnector depending on the platform.
#if !UNITY_EDITOR && UNITY_WEBGL
			_beaconConnector = new BeaconConnectorWebGl();
#else
			_beaconConnector = new BeaconConnectorDotNet(EventManager, TezosConfig.Instance.Network.ToString(),
				TezosConfig.Instance.RpcBaseUrl, WalletProviderType.beacon, _dAppMetadata);

			Connect(WalletProviderType.beacon, false);

			EventManager.PairingCompleted += OnPairingCompleted;
#endif
			EventManager.HandshakeReceived += OnHandshakeReceived;
			EventManager.AccountConnected += OnAccountConnected;
			EventManager.PayloadSigned += OnPayloadSigned;
			EventManager.ContractCallInjected += OnContractCallInjected;
		}

		private void OnPairingCompleted(PairingDoneData _)
		{
			_beaconConnector.RequestTezosPermission(TezosConfig.Instance.Network.ToString(),
				TezosConfig.Instance.RpcBaseUrl);
		}

		private void OnContractCallInjected(OperationResult transaction)
		{
			var transactionHash = transaction.TransactionHash;

			CoroutineRunner.Instance.StartWrappedCoroutine(
				new CoroutineWrapper<object>(TrackTransaction(transactionHash)));
		}

		private void OnPayloadSigned(SignResult payload)
		{
			_signature = payload.Signature;
		}

		private void OnAccountConnected(AccountInfo account)
		{
			_pubKey = account.PublicKey;
		}

		private void OnHandshakeReceived(HandshakeData handshake)
		{
			_handshake = handshake.PairingData;

#if UNITY_ANDROID || UNITY_IOS
			if (_withRedirectToWallet)
			{
				OpenWallet();
			}
#endif
		}

#if UNITY_ANDROID || UNITY_IOS
		private void OpenWallet()
		{
			Application.OpenURL($"tezos://?type=tzip10&data={_handshake}");
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

			var contractCallCompletedEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeContractCallCompleted,
				Data = JsonUtility.ToJson(operationResult)
			};

			EventManager.HandleEvent(JsonUtility.ToJson(contractCallCompletedEvent));
		}
	}

}