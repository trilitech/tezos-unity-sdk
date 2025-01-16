using System;
using System.Collections.Generic;
using System.Text.Json;
using Tezos.Configs;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MainThreadDispatcher;
using Tezos.MessageSystem;
using Tezos.Operation;
using Tezos.Request;
using Tezos.SocialLoginProvider;
using Tezos.WalletProvider;
using Tezos.Token;
using OperationRequest = Tezos.Operation.OperationRequest;

namespace Tezos.API
{
	public static partial class TezosAPI
	{
		public static event Action<WalletProviderData>  WalletConnected;
		public static event Action                      WalletDisconnected;
		public static event Action<SocialProviderData>  SocialLoggedIn;
		public static event Action                      SocialLoggedOut;
		public static event Action<OperationResponse>   OperationResulted;
		public static event Action<SignPayloadResponse> SigningResulted;
		public static event Action<string>              PairingRequested;

		private static UniTaskCompletionSource<bool> _sdkInitializedTcs;

		private static Rpc                      _rpc;
		private static WalletProviderController _walletProviderController;
		private static SocialProviderController _socialProviderController;

		static TezosAPI() => _sdkInitializedTcs = new();

		public static void Init(IContext context, WalletProviderController walletProviderController, SocialProviderController socialProviderController)
		{
			context.MessageSystem.AddListener<SdkInitializedCommand>(OnSDKInitialized);

			var tezosConfig = ConfigGetter.GetOrCreateConfig<TezosConfig>();

			_rpc                      = new(tezosConfig.RequestTimeoutSeconds);
			_walletProviderController = walletProviderController;
			_socialProviderController = socialProviderController;

			_walletProviderController.WalletConnected    += OnWalletConnected;
			_walletProviderController.WalletDisconnected += OnWalletDisconnected;
			_walletProviderController.PairingRequested   += OnPairingRequested;
		}

		private static void OnWalletConnected(WalletProviderData walletProviderData)
		{
			TezosLogger.LogInfo($"TezosAPI.OnWalletConnected. walletProviderData is null: {walletProviderData is null}, walletProviderData.WalletAddress: {walletProviderData.WalletAddress}");
			WalletConnected?.Invoke(walletProviderData);
		}

		private static void OnWalletDisconnected()                              => WalletDisconnected?.Invoke();
		private static void OnPairingRequested(string              pairingData) => PairingRequested?.Invoke(pairingData);
		private static void OnSDKInitialized(SdkInitializedCommand command)     => _sdkInitializedTcs.TrySetResult(command.GetData());

#region APIs

		public static UniTask WaitUntilSDKInitialized()
		{
			TezosLogger.LogDebug($"WaitUntilSDKInitialized");
			return _sdkInitializedTcs.Task;
		}

		public static T GetWalletProvider<T>() where T : IWalletProvider      => (T)_walletProviderController.GetWalletProvider<T>();
		public static T GetSocialProvider<T>() where T : ISocialLoginProvider => (T)_socialProviderController.GetSocialProvider<T>();

		public static bool               IsConnected()             => IsWalletConnected() || IsSocialLoggedIn();
		public static string             GetConnectionAddress()    => IsWalletConnected() ? GetWalletConnectionData().WalletAddress : IsSocialLoggedIn() ? GetSocialLoginData().WalletAddress : string.Empty;
		public static bool               IsWalletConnected()       => _walletProviderController.IsConnected;
		public static WalletProviderData GetWalletConnectionData() => _walletProviderController.GetWalletProviderData();

		public static async UniTask<WalletProviderData> ConnectWallet(WalletProviderData walletProviderData)
		{
			if (IsSocialLoggedIn()) throw new AlreadyConnectedException("Can not connect wallet since there is already a social login.");
			if (IsWalletConnected()) return GetWalletConnectionData();

			var result = await _walletProviderController.Connect(walletProviderData);
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(
			                                                        () =>
			                                                        {
				                                                        WalletConnected?.Invoke(result);
			                                                        }
			                                                       );
			return result;
		}

		public static async UniTask<bool> Disconnect()
		{
			if (!IsConnected())
			{
				TezosLogger.LogWarning("No connection found to disconnect");
				return false;
			}

			if (IsWalletConnected())
			{
				var result = await _walletProviderController.Disconnect();
				await UnityMainThreadDispatcher.Instance().EnqueueAsync(
				                                                        () =>
				                                                        {
					                                                        WalletDisconnected?.Invoke();
				                                                        }
				                                                       );
				return result;
			}

			if (IsSocialLoggedIn())
			{
				var result = await _socialProviderController.LogOut();
				await UnityMainThreadDispatcher.Instance().EnqueueAsync(
				                                                        () =>
				                                                        {
					                                                        SocialLoggedOut?.Invoke();
				                                                        }
				                                                       );
				return result;
			}

			throw new ConnectionRequiredException("Disconnection failed because no connection found");
		}

		public static bool               IsSocialLoggedIn()   => _socialProviderController.IsConnected;
		public static SocialProviderData GetSocialLoginData() => _socialProviderController.GetSocialProviderData();

		public static async UniTask<SocialProviderData> SocialLogIn(SocialProviderData socialProviderData)
		{
			if (IsWalletConnected()) throw new AlreadyConnectedException("Can not login since there is already a wallet connected");
			if (IsSocialLoggedIn()) return GetSocialLoginData();

			var result = await _socialProviderController.LogIn(socialProviderData);
			SocialLoggedIn?.Invoke(result);
			return result;
		}

		public static async UniTask<OperationResponse> RequestOperation(OperationRequest walletOperationRequest)
		{
			var result = await ProviderFactory.GetConnectedProviderController().RequestOperation(walletOperationRequest);
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(
			                                                        () =>
			                                                        {
				                                                        OperationResulted?.Invoke(result);
			                                                        }
			                                                       );
			return result;
		}

		public static async UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest operationRequest)
		{
			var result = await ProviderFactory.GetConnectedProviderController().RequestSignPayload(operationRequest);
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(
			                                                        () =>
			                                                        {
				                                                        SigningResulted?.Invoke(result);
			                                                        }
			                                                       );
			return result;
		}

		/// <summary>
		/// Fetches the XTZ balance of a given wallet address asynchronously.
		/// </summary>
		public static UniTask<string> GetBalance()
		{
			if (!IsConnected()) throw new ConnectionRequiredException("Not connected");

			return ProviderFactory.GetConnectedProviderController().GetBalance();
		}

		public static UniTask       DeployContract(DeployContractRequest deployContractRequest)                      => ProviderFactory.GetConnectedProviderController().DeployContract(deployContractRequest);
		public static UniTask<T>    ReadView<T>(string                   contractAddress, string view, object data) => _rpc.PostRequest<T>(EndPoints.GetRunViewEndPoint(contractAddress, view), data);
		public static UniTask<T>    GetTokens<T>(string                  address,         int    limit = 100) => _rpc.GetRequest<T>(EndPoints.GetTokensEndPoint(address, limit));
		public static UniTask<T>    GetTokenMetadata<T>(string           tokenId)       => _rpc.GetRequest<T>(EndPoints.GetTokenMetadataEndPoint(tokenId));
		public static UniTask<bool> GetOperationStatus(string            operationHash) => _rpc.GetRequest<bool>(EndPoints.GetOperationStatusEndPoint(operationHash));

#endregion
	}
}