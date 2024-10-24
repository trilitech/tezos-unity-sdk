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

		public static UniTask DeployContract(DeployContractRequest deployContractRequest) => ProviderFactory.GetConnectedProviderController().DeployContract(deployContractRequest);

		/// <summary>
		/// Fetches the XTZ balance of a given wallet address asynchronously.
		/// </summary>
		public static UniTask<string> GetBalance()
		{
			if (!IsConnected()) throw new ConnectionRequiredException("Not connected");

			return ProviderFactory.GetConnectedProviderController().GetBalance();
		}

		public static UniTask<T> ReadView<T>(string contractAddress, string entrypoint, string input) => _rpc.RunView<T>(contractAddress, entrypoint, input);

		public static UniTask<IEnumerable<TokenBalance>> GetTokensForOwner(
			string              owner,
			bool                withMetadata,
			long                maxItems,
			TokensForOwnerOrder orderBy
			)
		{
			var sort = orderBy switch
			           {
				           TokensForOwnerOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				           TokensForOwnerOrder.ByLastTimeAsc byLastTimeAsc =>
					           $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				           TokensForOwnerOrder.ByLastTimeDesc byLastTimeDesc =>
					           $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				           _ => string.Empty
			           };
			return _rpc.GetRequest<IEnumerable<TokenBalance>>(
			                                                  EndPoints.GetTokensForOwnerEndPoint(
			                                                                                      owner, withMetadata,
			                                                                                      maxItems, sort
			                                                                                     )
			                                                 );
		}

		public static UniTask<IEnumerable<TokenBalance>> GetOwnersForToken(
			string              contractAddress,
			uint                tokenId,
			long                maxItems,
			OwnersForTokenOrder orderBy
			)
		{
			var sort = orderBy switch
			           {
				           OwnersForTokenOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				           OwnersForTokenOrder.ByBalanceAsc byBalanceAsc =>
					           $"sort.asc=balance&offset.pg={byBalanceAsc.page}",
				           OwnersForTokenOrder.ByBalanceDesc byBalanceDesc =>
					           $"sort.desc=balance&offset.pg={byBalanceDesc.page}",
				           OwnersForTokenOrder.ByLastTimeAsc byLastTimeAsc =>
					           $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				           OwnersForTokenOrder.ByLastTimeDesc byLastTimeDesc =>
					           $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				           _ => string.Empty
			           };
			return _rpc.GetRequest<IEnumerable<TokenBalance>>(
			                                                  EndPoints.GetOwnersForTokenEndPoint(
			                                                                                      contractAddress,
			                                                                                      tokenId, maxItems,
			                                                                                      sort
			                                                                                     )
			                                                 );
		}

		public static UniTask<IEnumerable<TokenBalance>> GetOwnersForContract(
			string                 contractAddress,
			long                   maxItems,
			OwnersForContractOrder orderBy
			)
		{
			var sort = orderBy switch
			           {
				           OwnersForContractOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				           OwnersForContractOrder.ByLastTimeAsc byLastTimeAsc =>
					           $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				           OwnersForContractOrder.ByLastTimeDesc byLastTimeDesc =>
					           $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				           _ => string.Empty
			           };
			return _rpc.GetRequest<IEnumerable<TokenBalance>>(
			                                                  EndPoints.GetOwnersForContractEndPoint(
			                                                                                         contractAddress,
			                                                                                         maxItems, sort
			                                                                                        )
			                                                 );
		}

		public static UniTask<bool> IsHolderOfContract(string wallet, string contractAddress) => _rpc.GetRequest<bool>(EndPoints.GetIsHolderOfContractEndPoint(wallet, contractAddress));

		public static UniTask<bool> IsHolderOfToken(string wallet, string contractAddress, uint tokenId) => _rpc.GetRequest<bool>(EndPoints.GetIsHolderOfTokenEndPoint(wallet, contractAddress, tokenId));

		public static UniTask<JsonElement> GetTokenMetadata(
			string contractAddress,
			uint   tokenId
			) => _rpc.GetRequest<JsonElement>(EndPoints.GetTokenMetadataEndPoint(contractAddress, tokenId));

		public static UniTask<JsonElement> GetContractMetadata(
			string contractAddress
			) => _rpc.GetRequest<JsonElement>(EndPoints.GetContractMetadataEndPoint(contractAddress));

		public static UniTask<IEnumerable<TokenData>> GetTokensForContract(
			string                 contractAddress,
			bool                   withMetadata,
			long                   maxItems,
			TokensForContractOrder orderBy
			)
		{
			TezosLogger.LogDebug($"Getting tokens for contract: {contractAddress}");
			var sort = orderBy switch
			           {
				           TokensForContractOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				           TokensForContractOrder.ByLastTimeAsc byLastTimeAsc =>
					           $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				           TokensForContractOrder.ByLastTimeDesc byLastTimeDesc =>
					           $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				           TokensForContractOrder.ByHoldersCountAsc byHoldersCountAsc =>
					           $"sort.asc=holdersCount&offset.pg={byHoldersCountAsc.page}",
				           TokensForContractOrder.ByHoldersCountDesc byHoldersCountDesc =>
					           $"sort.desc=holdersCount&offset.pg={byHoldersCountDesc.page}",
				           _ => string.Empty
			           };
			return _rpc.GetRequest<IEnumerable<TokenData>>(
			                                               EndPoints.GetTokensForContractEndPoint(
			                                                                                      contractAddress,
			                                                                                      withMetadata,
			                                                                                      maxItems, sort
			                                                                                     )
			                                              );
		}

		public static UniTask<bool> GetOperationStatus(string operationHash) => _rpc.GetRequest<bool>(EndPoints.GetOperationStatusEndPoint(operationHash));

		public static UniTask<int> GetLatestBlockLevel() => _rpc.GetRequest<int>(EndPoints.GetLatestBlockLevelEndPoint());

		public static UniTask<int> GetAccountCounter(string address) => _rpc.GetRequest<int>(EndPoints.GetAccountCounterEndPoint(address));

		public static UniTask<IEnumerable<FA2Token>> GetOriginatedContractsForOwner(
			string                           creator,
			string                           codeHash,
			long                             maxItems,
			OriginatedContractsForOwnerOrder orderBy
			)
		{
			TezosLogger.LogDebug($"API.GetOriginatedContractsForOwner: creator={creator}, codeHash={codeHash}");

			var sort = orderBy switch
			           {
				           OriginatedContractsForOwnerOrder.Default byDefault                     => $"sort.asc=id&offset.cr={byDefault.lastId}",
				           OriginatedContractsForOwnerOrder.ByLastActivityTimeAsc byLastTimeAsc   => $"sort.asc=lastActivity&offset.pg={byLastTimeAsc.page}",
				           OriginatedContractsForOwnerOrder.ByLastActivityTimeDesc byLastTimeDesc => $"sort.desc=lastActivity&offset.pg={byLastTimeDesc.page}",
				           _                                                                      => string.Empty
			           };

			var url = $"contracts?creator={creator}&tzips.any=fa2&codeHash={codeHash}&" +
			          $"select=address,tokensCount as tokens_count,lastActivity,lastActivityTime as last_activity_time,id&{sort}&limit={maxItems}";

			return _rpc.GetRequest<IEnumerable<FA2Token>>(url);
		}

#endregion
	}
}