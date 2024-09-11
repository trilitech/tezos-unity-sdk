using System;
using System.Threading.Tasks;
using TezosSDK.Logger;
using TezosSDK.MessageSystem;
using TezosSDK.WalletProvider;

namespace TezosSDK.API
{
	public class TezosAPI
	{
		private static TaskCompletionSource<bool>               _alreadyConnectedOrLoggedInTcs    = new();
		private static TaskCompletionSource<WalletProviderData> _walletConnectedTcs    = new();
		private static TaskCompletionSource<WalletProviderData> _walletDisconnectedTcs = new();
		private static TaskCompletionSource<SocialProviderData> _socialLoggedInTcs     = new();
		private static TaskCompletionSource<SocialProviderData> _socialLoggedOutTcs    = new();
		
		public static event Action<WalletProviderData> WalletConnected;
		public static event Action<WalletProviderData> WalletDisconnected;
		public static event Action<SocialProviderData> SocialLoggedIn;
		public static event Action<SocialProviderData> SocialLoggedOut;

		private static IContext _context;
		
		public static void Init(IContext context)
		{
			_context = context;
			_context.MessageSystem.AddListener<WalletConnectedCommand>(OnWalletConnected);
			_context.MessageSystem.AddListener<WalletDisconnectedCommand>(OnWalletDisconnected);
			_context.MessageSystem.AddListener<SocialLoggedInCommand>(OnSocialLoggedIn);
			_context.MessageSystem.AddListener<SocialLoggedOutCommand>(OnSocialLoggedOut);
			_context.MessageSystem.AddListener<WalletAlreadyConnectedResultCommand>(OnWalletAlreadyConnected);
			_context.MessageSystem.AddListener<SocialAlreadyLoggedInResultCommand>(OnSocialAlreadyLoggedIn);
		}

		private static void OnWalletAlreadyConnected(WalletAlreadyConnectedResultCommand command) => _alreadyConnectedOrLoggedInTcs.TrySetResult(command.GetData());
		private static void OnSocialAlreadyLoggedIn(SocialAlreadyLoggedInResultCommand command)   => _alreadyConnectedOrLoggedInTcs.TrySetResult(command.GetData());

		private static void OnWalletConnected(WalletConnectedCommand walletConnectedCommand)
		{
			WalletConnected?.Invoke(walletConnectedCommand.GetData());
			_walletConnectedTcs.TrySetResult(walletConnectedCommand.GetData());
		}

		private static void OnWalletDisconnected(WalletDisconnectedCommand walletDisconnectedCommand)
		{
			WalletDisconnected?.Invoke(walletDisconnectedCommand.GetData());
			_walletDisconnectedTcs.TrySetResult(walletDisconnectedCommand.GetData());
		}
		
		private static void OnSocialLoggedIn(SocialLoggedInCommand socialLoggedInCommand)
		{
			SocialLoggedIn?.Invoke(socialLoggedInCommand.GetData());
			_socialLoggedInTcs.TrySetResult(socialLoggedInCommand.GetData());
		}

		private static void OnSocialLoggedOut(SocialLoggedOutCommand socialLoggedOutCommand)
		{
			SocialLoggedOut?.Invoke(socialLoggedOutCommand.GetData());
			_socialLoggedOutTcs.TrySetResult(socialLoggedOutCommand.GetData());
		}
		
#region APIs
		public static async Task<bool> IsConnected()
		{
			_context.MessageSystem.InvokeMessage(new WalletAlreadyConnectedRequestCommand());
			_context.MessageSystem.InvokeMessage(new SocialAlreadyLoggedInRequestCommand());
			return await _alreadyConnectedOrLoggedInTcs.Task;
		}

		public static async Task<WalletProviderData> ConnectWallet(WalletProviderData walletProviderData)
		{
			if (_walletConnectedTcs != null && !_walletConnectedTcs.Task.IsCompleted)
			{
				TezosLogger.LogWarning("Wallet connection already in progress");
				return await _walletConnectedTcs.Task;
			}
			
			_walletConnectedTcs = new();
			_context.MessageSystem.InvokeMessage(new WalletConnectionRequestCommand(walletProviderData));
			return await _walletConnectedTcs.Task;
		}
		
		public static async Task<WalletProviderData> DisconnectWallet(WalletProviderData walletProviderData = null)
		{
			if (_walletDisconnectedTcs != null && !_walletDisconnectedTcs.Task.IsCompleted)
			{
				TezosLogger.LogWarning("Wallet disconnection already in progress");
				return await _walletDisconnectedTcs.Task;
			}
			
			_walletDisconnectedTcs = new();
			_context.MessageSystem.InvokeMessage(new WalletDisconnectionRequestCommand(walletProviderData));
			return await _walletDisconnectedTcs.Task;
		}

		public static async Task<SocialProviderData> SocialLogIn(SocialProviderData socialProviderData)
		{
			if (_socialLoggedInTcs != null && !_socialLoggedInTcs.Task.IsCompleted)
			{
				TezosLogger.LogWarning("Social login already in progress");
				return await _socialLoggedInTcs.Task;
			}
			
			_socialLoggedInTcs = new();
			_context.MessageSystem.InvokeMessage(new SocialLogInRequestCommand(socialProviderData));
			return await _socialLoggedInTcs.Task;
		}
		
		public static async Task<SocialProviderData> SocialLogOut(SocialProviderData socialProviderData)
		{
			if (_socialLoggedOutTcs != null && !_socialLoggedOutTcs.Task.IsCompleted)
			{
				TezosLogger.LogWarning("Social logout already in progress");
				return await _socialLoggedOutTcs.Task;
			}
			
			_socialLoggedOutTcs = new();
			_context.MessageSystem.InvokeMessage(new SocialLogInRequestCommand(socialProviderData));
			return await _socialLoggedOutTcs.Task;
		}
		
		// public TezosAPI(TezosConfig config) : base(config.DataProvider)
		// {
		// 	Rpc = new Rpc(config);
		// }
		//
		// private Rpc Rpc { get; }
		//
		// public IEnumerator GetTezosBalance(Action<HttpResult<ulong>> callback, string address)
		// {
		// 	yield return Rpc.GetTzBalance(address, callback);
		// }
		//
		// public IEnumerator ReadView(string contractAddress, string entrypoint, string input, Action<HttpResult<JsonElement>> callback)
		// {
		// 	yield return Rpc.RunView(contractAddress, entrypoint, input, callback);
		// }
		//
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
