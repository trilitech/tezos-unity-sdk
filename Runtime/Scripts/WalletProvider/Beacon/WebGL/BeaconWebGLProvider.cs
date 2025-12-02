using System;
using Newtonsoft.Json;
using Tezos.Configs;
using Tezos.Cysharp;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MainThreadDispatcher;
using Tezos.MessageSystem;
using Tezos.Operation;
using Tezos.Request;
using UnityEngine;
using Object = UnityEngine.Object;
// ReSharper disable once RedundantUsingDirective
using System.Runtime.InteropServices;

namespace Tezos.WalletProvider
{
	public class UnifiedEvent
	{
		public string Data      { get; set; }
		public string EventType { get; set; }
	}

	/// <summary>
	///     WebGL implementation of the BeaconConnector.
	/// </summary>
	public class BeaconWebGLProvider : IWebGLProvider
	{
		public event Action<WalletProviderData> WalletConnected;
		public event Action                     WalletDisconnected;
		public event Action<string>             PairingRequested;

		public WalletType WalletType => WalletType.BEACON;

		private UniTaskCompletionSource<OperationResponse>   _operationTcs;
		private UniTaskCompletionSource<SignPayloadResponse> _signPayloadTcs;
		private UniTaskCompletionSource<WalletProviderData>  _walletConnectionTcs;
		private UniTaskCompletionSource<bool>                _walletDisconnectionTcs;

		private WebGLEventBridge _webGLEventBridge;
		private Rpc              _rpc;
		private TezosConfig      _tezosConfig;

		public UniTask Init()
		{
			_tezosConfig                           =  ConfigGetter.GetOrCreateConfig<TezosConfig>();
			_rpc                                   =  new(_tezosConfig.RequestTimeoutSeconds);
			_webGLEventBridge                      =  new GameObject("BeaconWebGLEventBridge").AddComponent<WebGLEventBridge>();
			_webGLEventBridge.EventReceived        += data => UnityMainThreadDispatcher.Instance().Enqueue(() => OnEventReceived(data));
			_webGLEventBridge.gameObject.hideFlags =  HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(_webGLEventBridge);

			JsUnityReadyEvent();
			return UniTask.CompletedTask;
		}

		public async UniTask<string> GetBalance(string walletAddress) => await _rpc.GetRequest<string>(EndPoints.GetBalanceEndPoint(walletAddress));

		private void OnEventReceived(string jsonEventData)
		{
			TezosLogger.LogDebug($"jsonEventData: {jsonEventData}");

			try
			{
				var eventData = JsonConvert.DeserializeObject<UnifiedEvent>(jsonEventData);

				TezosLogger.LogDebug($"Received event: {eventData.Data} - {eventData.EventType}");

				switch (eventData.EventType)
				{
					case "EventTypePairingRequest": PairingRequested?.Invoke(eventData.Data); break;
					case "EventTypePairingDone":    break;
					case "EventTypeWalletConnected":
					case "AccountConnected":
						var walletProviderData = JsonConvert.DeserializeObject<WalletProviderData>(eventData.Data);
						TezosLogger.LogInfo($"walletProviderData.WalletAddress:{walletProviderData.WalletAddress}");
						_walletConnectionTcs?.TrySetResult(walletProviderData);
						WalletConnected?.Invoke(walletProviderData);
						break;
					case "EventTypeWalletConnectionFailed":
					case "AccountConnectionFailed":
						_walletConnectionTcs.TrySetException(new WalletConnectionRejected("Wallet connection failed."));
						_walletConnectionTcs = null;
						break;
					case "EventTypeWalletDisconnected":
					case "AccountDisconnected":
						_walletDisconnectionTcs?.TrySetResult(true);
						WalletDisconnected?.Invoke();
						break;
					case "EventTypeOperationInjected": break;
					case "EventTypeOperationCompleted":
					case "ContractCallInjected":
						var operationResult = JsonConvert.DeserializeObject<OperationResponse>(eventData.Data);
						_operationTcs.TrySetResult(operationResult);
						break;
					case "EventTypeOperationFailed":
					case "ContractCallFailed":
						_operationTcs.TrySetException(new WalletOperationRejected("Wallet operation failed."));
						_operationTcs = null;
						break;
					case "EventTypePayloadSigned":
					case "PayloadSigned": _signPayloadTcs.TrySetResult(JsonConvert.DeserializeObject<SignPayloadResponse>(eventData.Data)); break;
					case "PayloadSignFailed":
						_signPayloadTcs.TrySetException(new WalletSignPayloadRejected("Payload signing failed."));
						_signPayloadTcs = null;
						break;
					case "EventTypeSDKInitialized": break;
					default:                        TezosLogger.LogWarning($"Unhandled event type: {eventData.EventType}"); break;
				}
			}
			catch (ArgumentException ex)
			{
				TezosLogger.LogError($"Error parsing event data: {ex.Message}\nData: {jsonEventData} - {ex.StackTrace}");
			}
		}

		public UniTask<WalletProviderData> Connect(WalletProviderData data)
		{
			if (_walletConnectionTcs != null && _walletConnectionTcs.Task.Status == UniTaskStatus.Pending) return _walletConnectionTcs.Task;

			_walletConnectionTcs = new();
			TezosLogger.LogDebug($"Connect method entered");
			var tezosConfig = ConfigGetter.GetOrCreateConfig<TezosConfig>();
			var appConfig   = ConfigGetter.GetOrCreateConfig<AppConfig>();
			var networkName = tezosConfig.Network == NetworkType.mainnet ? "mainnet" : "ghostnet"; // beacon dotnet sdk does not support shadownet
			JsInitWallet(
			             networkName,      tezosConfig.Rpc, WalletType.ToString().ToLower(), appConfig.AppName,
			             appConfig.AppUrl, appConfig.AppIcon
			            );

			JsConnectAccount();
			return _walletConnectionTcs.WithTimeout(_tezosConfig.RequestTimeoutSeconds * 1000, "Wallet connection task timeout.");
		}

		public async UniTask<bool> Disconnect()
		{
			if (_walletDisconnectionTcs != null && _walletDisconnectionTcs.Task.Status == UniTaskStatus.Pending) return await _walletDisconnectionTcs.Task;

			_walletDisconnectionTcs = new();

			TezosLogger.LogDebug("Disconnecting wallet");
			JsDisconnectAccount();
			// await UnityMainThreadDispatcher.Instance().EnqueueAsync(HandleDisconnection);
			return await _walletDisconnectionTcs.Task;
		}

		public async UniTask<OperationResponse> RequestOperation(OperationRequest operationRequest)
		{
			if (_operationTcs != null && _operationTcs.Task.Status == UniTaskStatus.Pending) return await _operationTcs.Task;

			_operationTcs = new();
			JsSendContractCall(operationRequest.Destination, operationRequest.Amount.ToString(), operationRequest.EntryPoint, operationRequest.Arg);
			return await _operationTcs.WithTimeout(_tezosConfig.RequestTimeoutSeconds * 1000, "Request operation task timeout.");
		}

		public async UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest signRequest)
		{
			if (_signPayloadTcs != null && _signPayloadTcs.Task.Status == UniTaskStatus.Pending) return await _signPayloadTcs.Task;

			_signPayloadTcs = new();
			JsSignPayload((int)signRequest.SigningType, signRequest.Payload);
			return await _signPayloadTcs.WithTimeout(_tezosConfig.RequestTimeoutSeconds * 1000, "Sign payload task timeout.");
		}

		public UniTask DeployContract(DeployContractRequest originationRequest)
		{
			TezosLogger.LogDebug("RequestContractOrigination - BeaconWebGL");
			JsRequestContractOrigination(originationRequest.Script, originationRequest.DelegateAddress);
			return UniTask.CompletedTask;
		}

		public bool IsAlreadyConnected() { throw new NotImplementedException(); }

		public string GetWalletAddress() { return JsGetActiveAccountAddress(); }

#if UNITY_WEBGL && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern void JsInitWallet(string network, string rpc, string walletProvider, string appName, string appUrl, string iconUrl);

		[DllImport("__Internal")]
		private static extern void JsConnectAccount();

		[DllImport("__Internal")]
		private static extern void JsDisconnectAccount();

		[DllImport("__Internal")]
		private static extern void JsSendContractCall(string destination, string amount, string entryPoint, string arg);

		[DllImport("__Internal")]
		private static extern void JsSignPayload(int signingType, string payload);

		[DllImport("__Internal")]
		private static extern string JsGetActiveAccountAddress();

		[DllImport("__Internal")]
		private static extern string JsRequestContractOrigination(string script, string delegateAddress);

		[DllImport("__Internal")]
		private static extern string JsUnityReadyEvent();
#else

#region Stub functions

		private void JsRequestContractOrigination(string script, string delegateAddress) { }

		private void JsInitWallet(string network, string rpc, string toString, string metadataName, string metadataUrl, string metadataIcon) { }

		private void JsUnityReadyEvent() { }

		private void JsConnectAccount() { }

		private void JsDisconnectAccount() { }

		private void JsSendContractCall(string destination, string toString, string entryPoint, string input) { }

		private string JsGetActiveAccountAddress() { return ""; }

		private void JsSignPayload(int signingType, string payload) { }

#endregion

#endif
		public void Dispose() { }
	}
}