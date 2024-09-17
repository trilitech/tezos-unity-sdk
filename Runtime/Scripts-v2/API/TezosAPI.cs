using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Tezos.Logger;
using Tezos.MessageSystem;
using Tezos.WalletProvider;
using Tezos.Token;

namespace Tezos.API
{
	public static class TezosAPI
	{
		private static TaskCompletionSource<bool>               _alreadyConnectedOrLoggedInTcs = new();
		
		private static TaskCompletionSource<WalletProviderData> _walletConnectedTcs            = new();
		private static TaskCompletionSource<WalletProviderData> _walletDisconnectedTcs         = new();
		
		public static event Action<WalletProviderData> WalletConnected;
		public static event Action<WalletProviderData> WalletDisconnected;
		public static event Action<SocialProviderData> SocialLoggedIn;
		public static event Action<SocialProviderData> SocialLoggedOut;

		private static WalletProviderController  _walletProviderController;
		private static SocialLoginController     _socialLoginController;
		private static Rpc                       _rpc;

		public static void Init(IContext context, WalletProviderController walletProviderController, SocialLoginController socialLoginController)
		{
			TezosLogger.LogDebug($"TezosAPI starting to initialize");

			_rpc                      = new(5);
			_walletProviderController = walletProviderController;
			_socialLoginController    = socialLoginController;
			
			TezosLogger.LogDebug($"TezosAPI initialized");
		}
		
#region APIs
		public static bool IsWalletConnected() => _walletProviderController.IsWalletConnected();
		public static WalletProviderData GetWalletConnectionData() => _walletProviderController.GetWalletProviderData();
		public static Task<WalletProviderData> ConnectWallet(WalletProviderData walletProviderData) => _walletProviderController.Connect(walletProviderData);
		public static Task<bool> DisconnectWallet() => _walletProviderController.Disconnect();
		
		public static bool IsSocialLoggedIn() => _socialLoginController.IsSocialLoggedIn();
		public static SocialProviderData GetSocialLoginData() => _socialLoginController.GetSocialProviderData();
		public static Task<SocialProviderData> SocialLogIn(SocialProviderData socialProviderData) => _socialLoginController.LogIn(socialProviderData);
		public static Task<bool> SocialLogOut() => _socialLoginController.LogOut();

		public static Task RequestOperation(WalletOperationRequest walletOperationRequest) => _walletProviderController.RequestOperation(walletOperationRequest);

		public static Task RequestSignPayload(WalletSignPayloadRequest walletOperationRequest) => _walletProviderController.RequestSignPayload(walletOperationRequest);

		public static Task RequestOriginateContract(WalletOriginateContractRequest walletOriginateContractRequest) => _walletProviderController.RequestOriginateContract(walletOriginateContractRequest);

		/// <summary>
		/// Fetches the XTZ balance of a given wallet address asynchronously.
		/// </summary>
		public static async Task<ulong> GetXTZBalance()
		{
			if (string.IsNullOrWhiteSpace(GetSocialLoginData().WalletAddress))
				throw new ArgumentException("Wallet address cannot be null or empty", nameof(GetSocialLoginData));
			
			return await _rpc.GetRequest<ulong>(EndPoints.GetBalanceEndPoint(GetSocialLoginData().WalletAddress));
		}
		
		public static Task<T> ReadView<T>(string contractAddress, string entrypoint, string input) => _rpc.RunView<T>(contractAddress, entrypoint, input);
		
		public static Task<IEnumerable<TokenBalance>> GetTokensForOwner(string owner, bool withMetadata, long maxItems, TokensForOwnerOrder orderBy)
		{
			var sort = orderBy switch
			{
				TokensForOwnerOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				TokensForOwnerOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				TokensForOwnerOrder.ByLastTimeDesc byLastTimeDesc => $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				_ => string.Empty
			};

			return _rpc.GetRequest<IEnumerable<TokenBalance>>(EndPoints.GetTokensForOwnerEndPoint(owner, withMetadata, maxItems, sort));
		}
		
		public static Task<IEnumerable<TokenBalance>> GetOwnersForToken(
			string contractAddress,
			uint tokenId,
			long maxItems,
			OwnersForTokenOrder orderBy)
		{
			var sort = orderBy switch
			{
				OwnersForTokenOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				OwnersForTokenOrder.ByBalanceAsc byBalanceAsc => $"sort.asc=balance&offset.pg={byBalanceAsc.page}",
				OwnersForTokenOrder.ByBalanceDesc byBalanceDesc => $"sort.desc=balance&offset.pg={byBalanceDesc.page}",
				OwnersForTokenOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				OwnersForTokenOrder.ByLastTimeDesc byLastTimeDesc => $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				_ => string.Empty
			};
		
			return _rpc.GetRequest<IEnumerable<TokenBalance>>(EndPoints.GetOwnersForTokenEndPoint(contractAddress, tokenId, maxItems, sort));
		}
		
		public static Task<IEnumerable<TokenBalance>> GetOwnersForContract(Action<HttpResult<IEnumerable<TokenBalance>>> callback, string contractAddress, long maxItems, OwnersForContractOrder orderBy)
		{
			var sort = orderBy switch
			{
				OwnersForContractOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				OwnersForContractOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				OwnersForContractOrder.ByLastTimeDesc byLastTimeDesc => $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				_ => string.Empty
			};
		
			return _rpc.GetRequest<IEnumerable<TokenBalance>>(EndPoints.GetOwnersForContractEndPoint(contractAddress, maxItems, sort));
		}
		
		public static Task<bool> IsHolderOfContract(string wallet, string contractAddress) => _rpc.GetRequest<bool>(EndPoints.GetIsHolderOfContractEndPoint(wallet, contractAddress));
		public static Task<bool> IsHolderOfToken(string wallet, string contractAddress, uint tokenId) => _rpc.GetRequest<bool>(EndPoints.GetIsHolderOfTokenEndPoint(wallet, contractAddress, tokenId));
		
		public static Task<JsonElement> GetTokenMetadata(Action<HttpResult<JsonElement>> callback, string contractAddress, uint tokenId) => _rpc.GetRequest<JsonElement>(EndPoints.GetTokenMetadataEndPoint(contractAddress, tokenId));
		
		public static Task<JsonElement> GetContractMetadata(Action<HttpResult<JsonElement>> callback, string contractAddress) => _rpc.GetRequest<JsonElement>(EndPoints.GetContractMetadataEndPoint(contractAddress));
		
		public static Task<IEnumerable<TokenData>> GetTokensForContract(
			string contractAddress,
			bool withMetadata,
			long maxItems,
			TokensForContractOrder orderBy)
		{
			TezosLogger.LogDebug($"Getting tokens for contract: {contractAddress}");
		
			var sort = orderBy switch
			{
				TokensForContractOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				TokensForContractOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				TokensForContractOrder.ByLastTimeDesc byLastTimeDesc => $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				TokensForContractOrder.ByHoldersCountAsc byHoldersCountAsc => $"sort.asc=holdersCount&offset.pg={byHoldersCountAsc.page}",
				TokensForContractOrder.ByHoldersCountDesc byHoldersCountDesc => $"sort.desc=holdersCount&offset.pg={byHoldersCountDesc.page}",
				_ => string.Empty
			};
		
			return _rpc.GetRequest<IEnumerable<TokenData>>(EndPoints.GetTokensForContractEndPoint(contractAddress, withMetadata, maxItems, sort));
		}
		
		public static Task<bool> GetOperationStatus(string operationHash) => _rpc.GetRequest<bool>(EndPoints.GetOperationStatusEndPoint(operationHash));

		public static Task<int> GetLatestBlockLevel() => _rpc.GetRequest<int>(EndPoints.GetLatestBlockLevelEndPoint());

		public static Task<int> GetAccountCounter(string address) => _rpc.GetRequest<int>(EndPoints.GetAccountCounterEndPoint(address));
		
		// public static IEnumerator GetOriginatedContractsForOwner(
		// 	Action<HttpResult<IEnumerable<TokenContract>>> callback,
		// 	string creator,
		// 	string codeHash,
		// 	long maxItems,
		// 	OriginatedContractsForOwnerOrder orderBy)
		// {
		// 	TezosLogger.LogDebug($"API.GetOriginatedContractsForOwner: creator={creator}, codeHash={codeHash}");
		//
		// 	var sort = orderBy switch
		// 	{
		// 		OriginatedContractsForOwnerOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
		// 		OriginatedContractsForOwnerOrder.ByLastActivityTimeAsc byLastTimeAsc => $"sort.asc=lastActivity&offset.pg={byLastTimeAsc.page}",
		// 		OriginatedContractsForOwnerOrder.ByLastActivityTimeDesc byLastTimeDesc => $"sort.desc=lastActivity&offset.pg={byLastTimeDesc.page}",
		// 		_ => string.Empty
		// 	};
		//
		// 	var url = $"contracts?creator={creator}&tzips.any=fa2&codeHash={codeHash}&" +
		// 	          "select=address,tokensCount as tokens_count,lastActivity,lastActivityTime as last_activity_time" + $",id&{sort}&limit={maxItems}";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }

		#endregion
	}
}
