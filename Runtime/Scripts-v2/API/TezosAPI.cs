using System;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Operation; // todo: We shouldn't use beacon here. Its better to create our own classes and mimic same data
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MainThreadDispatcher;
using Tezos.MessageSystem;
using Tezos.WalletProvider;
using Tezos.Token;

namespace Tezos.API
{
	public static partial class TezosAPI
	{
		public static event Action<WalletProviderData> WalletConnected;
		public static event Action                     WalletDisconnected;
		public static event Action<SocialProviderData> SocialLoggedIn;
		public static event Action                     SocialLoggedOut;
		public static event Action<OperationResponse>  OperationResulted;
		public static event Action<WalletProviderData> SigningResulted;
		public static event Action<string>             PairingRequested;

		private static UniTaskCompletionSource<bool> _sdkInitializedTcs;

		private static WalletProviderController _walletProviderController;
		private static SocialLoginController    _socialLoginController;
		private static Rpc                      _rpc;

		static TezosAPI() => _sdkInitializedTcs = new();

		public static void Init(
			IContext                 context,
			WalletProviderController walletProviderController,
			SocialLoginController    socialLoginController
			)
		{
			context.MessageSystem.AddListener<SdkInitializedCommand>(OnSDKInitialized);
			_rpc                                       =  new(5);
			_walletProviderController                  =  walletProviderController;
			_socialLoginController                     =  socialLoginController;
			_walletProviderController.PairingRequested += OnPairingRequested;
		}

		private static void OnSDKInitialized(SdkInitializedCommand command)     => _sdkInitializedTcs.TrySetResult(command.GetData());
		private static void OnPairingRequested(string              pairingData) => PairingRequested?.Invoke(pairingData);

#region APIs

		public static UniTask WaitUntilSDKInitialized()
		{
			TezosLogger.LogDebug($"WaitUntilSDKInitialized");
			return _sdkInitializedTcs.Task;
		}

		public static bool               IsWalletConnected()       => _walletProviderController.IsWalletConnected();
		public static WalletProviderData GetWalletConnectionData() => _walletProviderController.GetWalletProviderData();

		public static async UniTask<WalletProviderData> ConnectWallet(WalletProviderData walletProviderData)
		{
			var result = await _walletProviderController.Connect(walletProviderData);
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(
			                                                        () =>
			                                                        {
				                                                        WalletConnected?.Invoke(result);
			                                                        }
			                                                       );
			return result;
		}

		public static async UniTask<bool> DisconnectWallet()
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

		public static bool               IsSocialLoggedIn()   => _socialLoginController.IsSocialLoggedIn();
		public static SocialProviderData GetSocialLoginData() => _socialLoginController.GetSocialProviderData();

		public static async UniTask<SocialProviderData> SocialLogIn(SocialProviderData socialProviderData)
		{
			var result = await _socialLoginController.LogIn(socialProviderData);
			SocialLoggedIn?.Invoke(result);
			return result;
		}

		public static async UniTask<bool> SocialLogOut()
		{
			var result = await _socialLoginController.LogOut();
			SocialLoggedOut?.Invoke();
			return result;
		}

		public static async UniTask<OperationResponse> RequestOperation(WalletOperationRequest walletOperationRequest)
		{
			var result = await _walletProviderController.RequestOperation(walletOperationRequest);
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(
			                                                        () =>
			                                                        {
				                                                        OperationResulted?.Invoke(result);
			                                                        }
			                                                       );
			return result;
		}

		public static async UniTask<WalletProviderData> RequestSignPayload(WalletSignPayloadRequest walletOperationRequest)
		{
			var result = await _walletProviderController.RequestSignPayload(walletOperationRequest);
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(
			                                                        () =>
			                                                        {
				                                                        SigningResulted?.Invoke(result);
			                                                        }
			                                                       );
			return result;
		}

		public static UniTask RequestContractOrigination(WalletOriginateContractRequest walletOriginateContractRequest) => _walletProviderController.RequestOriginateContract(walletOriginateContractRequest);

		/// <summary>
		/// Fetches the XTZ balance of a given wallet address asynchronously.
		/// </summary>
		public static async UniTask<ulong> GetXTZBalance()
		{
			if (string.IsNullOrWhiteSpace(GetWalletConnectionData().WalletAddress)) throw new ArgumentException("Wallet address cannot be null or empty", nameof(GetSocialLoginData));
			return await _rpc.GetRequest<ulong>(EndPoints.GetBalanceEndPoint(GetWalletConnectionData().WalletAddress));
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

		private static UniTask<IEnumerable<string>> GetOriginatedContractsForOwner(
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

			var url = $"contracts?creator={creator}&tzips.any=fa2&codeHash={codeHash}&"                                +
			          "select=address,tokensCount as tokens_count,lastActivity,lastActivityTime as last_activity_time" +
			          $",id&{sort}&limit={maxItems}";

			return _rpc.GetRequest<IEnumerable<string>>(url);
		}

#endregion
	}
}