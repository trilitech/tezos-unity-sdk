using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos.API
{

	public class TezosAPI : TezosHttpClient, ITezosAPI
	{
		public TezosAPI(TezosConfigSO config) : base(config.DataProvider)
		{
			Rpc = new Rpc(config);
		}

		private Rpc Rpc { get; }

		public IEnumerator GetTezosBalance(Action<Result<ulong>> callback, string address)
		{
			yield return Rpc.GetTzBalance(address, callback);
		}

		public IEnumerator ReadView(
			string contractAddress,
			string entrypoint,
			string input,
			Action<Result<JsonElement>> callback)
		{
			yield return Rpc.RunView(contractAddress, entrypoint, input, callback);
		}

		public IEnumerator GetTokensForOwner(
			Action<Result<IEnumerable<TokenBalance>>> callback,
			string owner,
			bool withMetadata,
			long maxItems,
			TokensForOwnerOrder orderBy)
		{
			var sort = orderBy switch
			{
				TokensForOwnerOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				TokensForOwnerOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
				TokensForOwnerOrder.ByLastTimeDesc byLastTimeDesc =>
					$"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				_ => string.Empty
			};

			var url = "tokens/balances?" + $"account={owner}&balance.ne=0&" +
			          "select=account.address as owner,balance,token.contract as token_contract," +
			          $"token.tokenId as token_id{(withMetadata ? ",token.metadata as token_metadata" : "")}," +
			          "lastTime as last_time,id&" + $"{sort}&limit={maxItems}";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetOwnersForToken(
			Action<Result<IEnumerable<TokenBalance>>> callback,
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
				OwnersForTokenOrder.ByLastTimeDesc byLastTimeDesc =>
					$"sort.desc=lastLevel&offset.pg={byLastTimeDesc.page}",
				_ => string.Empty
			};

			var url = "tokens/balances?" + $"token.contract={contractAddress}&balance.ne=0&token.tokenId={tokenId}&" +
			          "select=account.address as owner,balance,token.contract as token_contract," +
			          "token.tokenId as token_id,lastTime as last_time,id&" + $"{sort}&limit={maxItems}";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetOwnersForContract(
			Action<Result<IEnumerable<TokenBalance>>> callback,
			string contractAddress,
			long maxItems,
			OwnersForContractOrder orderBy)
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

			var url = "tokens/balances?" + $"token.contract={contractAddress}&balance.ne=0&" +
			          "select=account.address as owner,balance,token.contract as token_contract," +
			          "token.tokenId as token_id,id&" + $"{sort}&limit={maxItems}";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator IsHolderOfContract(Action<Result<bool>> callback, string wallet, string contractAddress)
		{
			var url = $"tokens/balances?account={wallet}&token.contract={contractAddress}&balance.ne=0&select=id";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator IsHolderOfToken(
			Action<Result<bool>> callback,
			string wallet,
			string contractAddress,
			uint tokenId)
		{
			var url =
				$"tokens/balances?account={wallet}&token.contract={contractAddress}&token.tokenId={tokenId}&balance.ne=0&select=id";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetTokenMetadata(Action<Result<JsonElement>> callback, string contractAddress, uint tokenId)
		{
			var url = $"tokens?contract={contractAddress}&tokenId={tokenId}&select=metadata";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetContractMetadata(Action<Result<JsonElement>> callback, string contractAddress)
		{
			var url = $"accounts/{contractAddress}?legacy=false";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetTokensForContract(
			Action<Result<IEnumerable<Token>>> callback,
			string contractAddress,
			bool withMetadata,
			long maxItems,
			TokensForContractOrder orderBy)
		{
			Logger.LogDebug($"Getting tokens for contract: {contractAddress}");

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

			var url = $"tokens?contract={contractAddress}&select=contract,tokenId as token_id" +
			          $"{(withMetadata ? ",metadata as token_metadata" : "")},holdersCount as holders_count,id," +
			          $"lastTime as last_time&{sort}&limit={maxItems}";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetOperationStatus(Action<Result<bool?>> callback, string operationHash)
		{
			var url = $"operations/{operationHash}/status";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetLatestBlockLevel(Action<Result<int>> callback)
		{
			var url = $"blocks/{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}/level";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetAccountCounter(Action<Result<int>> callback, string address)
		{
			var url = $"accounts/{address}/counter";

			yield return GetJsonCoroutine(url, callback);
		}

		public IEnumerator GetOriginatedContractsForOwner(
			Action<Result<IEnumerable<TokenContract>>> callback,
			string creator,
			string codeHash,
			long maxItems,
			OriginatedContractsForOwnerOrder orderBy)
		{
			Logger.LogDebug($"GetOriginatedContractsForOwner: creator={creator}, codeHash={codeHash}");

			var sort = orderBy switch
			{
				OriginatedContractsForOwnerOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
				OriginatedContractsForOwnerOrder.ByLastActivityTimeAsc byLastTimeAsc =>
					$"sort.asc=lastActivity&offset.pg={byLastTimeAsc.page}",
				OriginatedContractsForOwnerOrder.ByLastActivityTimeDesc byLastTimeDesc =>
					$"sort.desc=lastActivity&offset.pg={byLastTimeDesc.page}",
				_ => string.Empty
			};

			var url = $"contracts?creator={creator}&tzips.any=fa2&codeHash={codeHash}&" +
			          "select=address,tokensCount as tokens_count,lastActivity,lastActivityTime as last_activity_time" +
			          $",id&{sort}&limit={maxItems}";

			yield return GetJsonCoroutine(url, callback);
		}
	}

}