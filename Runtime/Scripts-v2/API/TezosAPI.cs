using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TezosSDK.Common;
using TezosSDK.Configs;
using TezosSDK.Logger;
using TezosSDK.MessageSystem;
using TezosSDK.WalletProvider;
using UnityEngine.Networking;

namespace TezosSDK.API
{
	public class TezosAPI
	{
		private static TaskCompletionSource<bool>               _alreadyConnectedOrLoggedInTcs = new();
		
		private static TaskCompletionSource<WalletProviderData> _walletConnectedTcs            = new();
		private static TaskCompletionSource<WalletProviderData> _walletDisconnectedTcs         = new();
		
		public static event Action<WalletProviderData> WalletConnected;
		public static event Action<WalletProviderData> WalletDisconnected;
		public static event Action<SocialProviderData> SocialLoggedIn;
		public static event Action<SocialProviderData> SocialLoggedOut;

		private static IContext                  _context;
		private static WalletProviderController  _walletProviderController;
		private static SocialLoginController     _socialLoginController;
		private static Rpc                       _rpc;

		public static void Init(IContext context, WalletProviderController walletProviderController, SocialLoginController socialLoginController)
		{
			TezosLogger.LogDebug($"TezosAPI starting to initialize");
			DataProviderConfig dataProviderConfig = ConfigGetter.GetOrCreateConfig<DataProviderConfig>();

			_rpc                      = new(dataProviderConfig.BaseUrl, 5);
			_context                  = context;
			_walletProviderController = walletProviderController;
			_socialLoginController    = socialLoginController;
			
			TezosLogger.LogDebug($"TezosAPI initialized");
		}
		
#region APIs
		public static bool IsWalletConnected() => _walletProviderController.IsWalletConnected();
		public static string GetWalletAddress() => _walletProviderController.GetWalletAddress();

		public static Task<WalletProviderData> ConnectWallet(WalletProviderData walletProviderData) => _walletProviderController.Connect(walletProviderData);

		public static Task<bool> DisconnectWallet() => _walletProviderController.Disconnect();
		public static bool IsSocialLoggedIn() => _socialLoginController.IsSocialLoggedIn();
		public static string GetSocialAddress() => _socialLoginController.GetWalletAddress();

		public static Task<SocialProviderData> SocialLogIn(SocialProviderData socialProviderData) => _socialLoginController.LogIn(socialProviderData);

		public static Task<bool> SocialLogOut() => _socialLoginController.LogOut();

		public static Task RequestOperation(WalletOperationRequest walletOperationRequest) => _walletProviderController.RequestOperation(walletOperationRequest);

		public static Task RequestSignPayload(WalletSignPayloadRequest walletOperationRequest) => _walletProviderController.RequestSignPayload(walletOperationRequest);

		public static Task RequestOriginateContract(WalletOriginateContractRequest walletOriginateContractRequest) => _walletProviderController.RequestOriginateContract(walletOriginateContractRequest);

		public static void GetXTZBalance(Action<long> callback)
		{
			UnityMainThreadDispatcher.Instance().StartCoroutine(_rpc.GetTzBalance(GetWalletAddress(), result =>
			{
				if (result.Success)
				{
					TezosLogger.LogDebug($"GetTzBalance result: {result.Data}");
					callback?.Invoke(result.Data);
				}
				else
				{
					TezosLogger.LogError($"GetTzBalance error: {result.ErrorMessage}");
					callback?.Invoke(0);
				}
			}));
		}
		
		public IEnumerator ReadView(string contractAddress, string entrypoint, string input, Action<HttpResult<JsonElement>> callback) => _rpc.RunView(contractAddress, entrypoint, input, callback);
		
		// public IEnumerator GetTokensForOwner(Action<HttpResult<IEnumerable<TokenBalance>>> callback, string owner, bool withMetadata, long maxItems, TokensForOwnerOrder orderBy)
		// {
		// 	var sort = orderBy switch
		// 	{
		// 		TokensForOwnerOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
		// 		TokensForOwnerOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
		// 		TokensForOwnerOrder.ByLastTimeDesc byLastTimeDesc => $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
		// 		_ => string.Empty
		// 	};
		//
		// 	var url = "tokens/balances?" + $"account={owner}&balance.ne=0&" + "select=account.address as owner,balance,token.contract as token_contract," +
		// 	          $"token.tokenId as token_id{(withMetadata ? ",token.metadata as token_metadata" : "")}," + "lastTime as last_time,id&" + $"{sort}&limit={maxItems}";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetOwnersForToken(
		// 	Action<HttpResult<IEnumerable<TokenBalance>>> callback,
		// 	string contractAddress,
		// 	uint tokenId,
		// 	long maxItems,
		// 	OwnersForTokenOrder orderBy)
		// {
		// 	var sort = orderBy switch
		// 	{
		// 		OwnersForTokenOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
		// 		OwnersForTokenOrder.ByBalanceAsc byBalanceAsc => $"sort.asc=balance&offset.pg={byBalanceAsc.page}",
		// 		OwnersForTokenOrder.ByBalanceDesc byBalanceDesc => $"sort.desc=balance&offset.pg={byBalanceDesc.page}",
		// 		OwnersForTokenOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
		// 		OwnersForTokenOrder.ByLastTimeDesc byLastTimeDesc => $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
		// 		_ => string.Empty
		// 	};
		//
		// 	var url = "tokens/balances?" + $"token.contract={contractAddress}&balance.ne=0&token.tokenId={tokenId}&" +
		// 	          "select=account.address as owner,balance,token.contract as token_contract," + "token.tokenId as token_id,lastTime as last_time,id&" +
		// 	          $"{sort}&limit={maxItems}";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetOwnersForContract(Action<HttpResult<IEnumerable<TokenBalance>>> callback, string contractAddress, long maxItems, OwnersForContractOrder orderBy)
		// {
		// 	var sort = orderBy switch
		// 	{
		// 		OwnersForContractOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
		// 		OwnersForContractOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
		// 		OwnersForContractOrder.ByLastTimeDesc byLastTimeDesc => $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
		// 		_ => string.Empty
		// 	};
		//
		// 	var url = "tokens/balances?" + $"token.contract={contractAddress}&balance.ne=0&" + "select=account.address as owner,balance,token.contract as token_contract," +
		// 	          "token.tokenId as token_id,id&" + $"{sort}&limit={maxItems}";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator IsHolderOfContract(Action<HttpResult<bool>> callback, string wallet, string contractAddress)
		// {
		// 	var url = $"tokens/balances?account={wallet}&token.contract={contractAddress}&balance.ne=0&select=id";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator IsHolderOfToken(Action<HttpResult<bool>> callback, string wallet, string contractAddress, uint tokenId)
		// {
		// 	var url = $"tokens/balances?account={wallet}&token.contract={contractAddress}&token.tokenId={tokenId}&balance.ne=0&select=id";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetTokenMetadata(Action<HttpResult<JsonElement>> callback, string contractAddress, uint tokenId)
		// {
		// 	var url = $"tokens?contract={contractAddress}&tokenId={tokenId}&select=metadata";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetContractMetadata(Action<HttpResult<JsonElement>> callback, string contractAddress)
		// {
		// 	var url = $"accounts/{contractAddress}?legacy=false";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetTokensForContract(
		// 	Action<HttpResult<IEnumerable<Token>>> callback,
		// 	string contractAddress,
		// 	bool withMetadata,
		// 	long maxItems,
		// 	TokensForContractOrder orderBy)
		// {
		// 	// //TezosLogger.LogDebug($"Getting tokens for contract: {contractAddress}");
		//
		// 	var sort = orderBy switch
		// 	{
		// 		TokensForContractOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
		// 		TokensForContractOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
		// 		TokensForContractOrder.ByLastTimeDesc byLastTimeDesc => $"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
		// 		TokensForContractOrder.ByHoldersCountAsc byHoldersCountAsc => $"sort.asc=holdersCount&offset.pg={byHoldersCountAsc.page}",
		// 		TokensForContractOrder.ByHoldersCountDesc byHoldersCountDesc => $"sort.desc=holdersCount&offset.pg={byHoldersCountDesc.page}",
		// 		_ => string.Empty
		// 	};
		//
		// 	var url = $"tokens?contract={contractAddress}&select=contract,tokenId as token_id" +
		// 	          $"{(withMetadata ? ",metadata as token_metadata" : "")},holdersCount as holders_count,id," + $"lastTime as last_time&{sort}&limit={maxItems}";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetOperationStatus(Action<HttpResult<bool>> callback, string operationHash)
		// {
		// 	var url = $"operations/{operationHash}/status";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetLatestBlockLevel(Action<HttpResult<int>> callback)
		// {
		// 	var url = $"blocks/{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}/level";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetAccountCounter(Action<HttpResult<int>> callback, string address)
		// {
		// 	var url = $"accounts/{address}/counter";
		//
		// 	yield return GetJsonCoroutine(url, callback);
		// }
		//
		// public IEnumerator GetOriginatedContractsForOwner(
		// 	Action<HttpResult<IEnumerable<TokenContract>>> callback,
		// 	string creator,
		// 	string codeHash,
		// 	long maxItems,
		// 	OriginatedContractsForOwnerOrder orderBy)
		// {
		// 	// //TezosLogger.LogDebug($"API.GetOriginatedContractsForOwner: creator={creator}, codeHash={codeHash}");
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
