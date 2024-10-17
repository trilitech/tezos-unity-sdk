using System;
using System.Runtime.InteropServices;
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

namespace Tezos.SocialLoginProvider
{
	public class UnifiedEvent
	{
		public string Data      { get; set; }
		public string EventType { get; set; }
	}
	
	public class KukaiWebGLProvider : IWebGLProvider
	{
		public event Action<SocialProviderData> WalletConnected;
		public event Action                     WalletDisconnected;
		public event Action<string>             PairingRequested;

		public SocialLoginType SocialLoginType => SocialLoginType.Kukai;

		private UniTaskCompletionSource<OperationResponse>   _operationTcs;
		private UniTaskCompletionSource<SignPayloadResponse> _signPayloadTcs;
		private UniTaskCompletionSource<SocialProviderData>  _logInTcs;
		private UniTaskCompletionSource<bool>                _logOutTcs;

		private KukaiWebGLEventBridge _webGLEventBridge;
		private Rpc                   _rpc;

		public UniTask Init(SocialProviderController socialProviderController)
		{
			_rpc                                   =  new(5);
			_webGLEventBridge                      =  new GameObject("KukaiWebGLEventBridge").AddComponent<KukaiWebGLEventBridge>();
			_webGLEventBridge.EventReceived        += data => UnityMainThreadDispatcher.Instance().Enqueue(() => OnEventReceived(data));
			_webGLEventBridge.gameObject.hideFlags =  HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(_webGLEventBridge);

			return UniTask.CompletedTask;
		}

		private void OnEventReceived(string jsonEventData)
		{
			TezosLogger.LogDebug($"jsonEventData: {jsonEventData}");

			try
			{
				var eventData = JsonConvert.DeserializeObject<UnifiedEvent>(jsonEventData);

				TezosLogger.LogDebug($"Received event: {eventData.Data} - {eventData.EventType}");

				switch (eventData.EventType)
				{
					case "EventTypePairingRequest":
						PairingRequested?.Invoke(eventData.Data);
						break;
					case "EventTypePairingDone": break;
					case "EventTypeWalletConnected":
					case "AccountConnected":
						var socialProviderData = JsonConvert.DeserializeObject<SocialProviderData>(eventData.Data);
						TezosLogger.LogInfo($"socialProviderData.WalletAddress:{socialProviderData.WalletAddress}");
						_logInTcs?.TrySetResult(socialProviderData);
						WalletConnected?.Invoke(socialProviderData);
						break;
					case "EventTypeWalletConnectionFailed":
					case "AccountConnectionFailed":
						_logInTcs.TrySetException(new SocialLogInFailed("Login failed."));
						_logInTcs = null;
						break;
					case "EventTypeWalletDisconnected":
					case "AccountDisconnected":
						_logOutTcs?.TrySetResult(true);
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
						_operationTcs.TrySetException(new SocialOperationFailed("Operation failed."));
						_operationTcs = null;
						break;
					case "EventTypePayloadSigned":
					case "PayloadSigned":
						_signPayloadTcs.TrySetResult(JsonConvert.DeserializeObject<SignPayloadResponse>(eventData.Data));
						break;
					case "PayloadSignFailed":
						_signPayloadTcs.TrySetException(new SocialSignPayloadFailed("Payload signing failed."));
						_signPayloadTcs = null;
						break;
					case "EventTypeSDKInitialized": break;
					default:
						TezosLogger.LogWarning($"Unhandled event type: {eventData.EventType}");
						break;
				}
			}
			catch (ArgumentException ex)
			{
				TezosLogger.LogError($"Error parsing event data: {ex.Message}\nData: {jsonEventData} - {ex.StackTrace}");
			}
		}

		public UniTask<SocialProviderData> LogIn(SocialProviderData data)
		{
			if (_logInTcs != null && _logInTcs.Task.Status == UniTaskStatus.Pending) return _logInTcs.Task;

			_logInTcs = new();
			TezosLogger.LogDebug($"Connect method entered");
			var dataProviderConfig = ConfigGetter.GetOrCreateConfig<DataProviderConfig>();
			var appConfig          = ConfigGetter.GetOrCreateConfig<AppConfig>();
			JsInitWallet(
			             dataProviderConfig.Network.ToString(), dataProviderConfig.BaseUrl, SocialLoginType.ToString().ToLower(), appConfig.AppName,
			             appConfig.AppUrl,                      appConfig.AppIcon
			            );

			JsConnectAccount();
			return _logInTcs.WithTimeout(30 * 1000, "Log in task timeout.");
		}

		public async UniTask<bool> LogOut()
		{
			if (_logOutTcs != null && _logOutTcs.Task.Status == UniTaskStatus.Pending) return await _logOutTcs.Task;

			_logOutTcs = new();

			TezosLogger.LogDebug("Disconnecting wallet");
			JsDisconnectAccount();
			// await UnityMainThreadDispatcher.Instance().EnqueueAsync(HandleDisconnection);
			return await _logOutTcs.Task;
		}

		public UniTask<string> GetBalance(string walletAddress) => _rpc.GetRequest<string>(EndPoints.GetBalanceEndPoint(walletAddress));

		public async UniTask<OperationResponse> RequestOperation(OperationRequest operationRequest)
		{
			if (_operationTcs != null && _operationTcs.Task.Status == UniTaskStatus.Pending) return await _operationTcs.Task;

			_operationTcs = new();
			JsSendContractCall(operationRequest.Destination, operationRequest.Amount, operationRequest.EntryPoint, operationRequest.Arg);
			return await _operationTcs.WithTimeout(10 * 1000, "Request operation task timeout.");
		}

		public async UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest signRequest)
		{
			if (_signPayloadTcs != null && _signPayloadTcs.Task.Status == UniTaskStatus.Pending) return await _signPayloadTcs.Task;

			_signPayloadTcs = new();
			JsSignPayload((int)signRequest.SigningType, signRequest.Payload);
			return await _signPayloadTcs.WithTimeout(10 * 1000, "Sign payload task timeout.");
		}

		public UniTask RequestContractOrigination(DeployContractRequest originationRequest)
		{
			TezosLogger.LogDebug("RequestContractOrigination - BeaconWebGL");
			JsRequestContractOrigination(originationRequest.Script, originationRequest.DelegateAddress);
			return UniTask.CompletedTask;
		}

		public bool IsLoggedIn() { throw new NotImplementedException(); }

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