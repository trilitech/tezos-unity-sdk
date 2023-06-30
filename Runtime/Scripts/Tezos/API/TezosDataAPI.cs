using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Dynamic.Json;
using TezosSDK.Helpers;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Operations;
using TezosSDK.Tezos.API.Models.Tokens;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos.API
{
    public class TezosDataAPI : HttpClient, ITezosDataAPI
    {
        private Rpc Rpc { get; }

        public TezosDataAPI(IDataProviderConfig config) : base(config)
        {
            Rpc = new Rpc(TezosConfig.Instance.RpcBaseUrl);
        }

        public IEnumerator GetTezosBalance(Action<ulong> callback, string address)
        {
            var getBalanceRequest = Rpc.GetTzBalance<ulong>(address);
            return new CoroutineWrapper<ulong>(getBalanceRequest, callback);
        }

        public IEnumerator ReadView(string contractAddress,
            string entrypoint,
            string input,
            Action<JsonElement> callback)
        {
            var runViewOp = Rpc.RunView<JsonElement>(contractAddress, entrypoint, input);

            return new CoroutineWrapper<JsonElement>(runViewOp, result =>
            {
                if (result.ValueKind != JsonValueKind.Null && result.ValueKind != JsonValueKind.Undefined &&
                    result.TryGetProperty("data", out var val))
                    callback(val);
                else
                    Logger.LogError("Can't parse response from run_script_view query");
            });
        }

        public IEnumerator GetTokensForOwner(
            Action<IEnumerable<TokenBalance>> callback,
            string owner,
            bool withMetadata,
            long maxItems,
            TokensForOwnerOrder orderBy)
        {
            var sort = orderBy switch
            {
                TokensForOwnerOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
                TokensForOwnerOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
                TokensForOwnerOrder.ByLastTimeDesc ByLastTimeDesc =>
                    $"sort.desc=lastLevel&offset.pg={ByLastTimeDesc.page}",
                _ => string.Empty
            };

            var url = "tokens/balances?" +
                      $"account={owner}&balance.ne=0&" +
                      "select=account.address as owner,balance,token.contract as token_contract," +
                      $"token.tokenId as token_id{(withMetadata ? ",token.metadata as token_metadata" : "")}," +
                      "lastTime as last_time,id&" +
                      $"{sort}&limit={maxItems}";

            var requestRoutine = GetJson<IEnumerable<TokenBalance>>(url);
            return new CoroutineWrapper<IEnumerable<TokenBalance>>(requestRoutine, callback);
        }

        public IEnumerator GetOwnersForToken(
            Action<IEnumerable<TokenBalance>> callback,
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

            var url = "tokens/balances?" +
                      $"token.contract={contractAddress}&balance.ne=0&token.tokenId={tokenId}&" +
                      "select=account.address as owner,balance,token.contract as token_contract," +
                      "token.tokenId as token_id,lastTime as last_time,id&" +
                      $"{sort}&limit={maxItems}";

            var requestRoutine = GetJson<IEnumerable<TokenBalance>>(url);
            return new CoroutineWrapper<IEnumerable<TokenBalance>>(requestRoutine, callback);
        }

        public IEnumerator GetOwnersForContract(
            Action<IEnumerable<TokenBalance>> callback,
            string contractAddress,
            long maxItems,
            OwnersForContractOrder orderBy)
        {
            var sort = orderBy switch
            {
                OwnersForContractOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
                OwnersForContractOrder.ByLastTimeAsc byLastTimeAsc =>
                    $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
                OwnersForContractOrder.ByLastTimeDesc ByLastTimeDesc =>
                    $"sort.desc=lastLevel&offset.pg={ByLastTimeDesc.page}",
                _ => string.Empty
            };

            var url = "tokens/balances?" +
                      $"token.contract={contractAddress}&balance.ne=0&" +
                      "select=account.address as owner,balance,token.contract as token_contract," +
                      "token.tokenId as token_id,id&" +
                      $"{sort}&limit={maxItems}";

            var requestRoutine = GetJson<IEnumerable<TokenBalance>>(url);
            return new CoroutineWrapper<IEnumerable<TokenBalance>>(requestRoutine, callback);
        }

        public IEnumerator IsHolderOfContract(
            Action<bool> callback,
            string wallet,
            string contractAddress)
        {
            var requestRoutine =
                GetJson($"tokens/balances?account={wallet}&token.contract={contractAddress}&balance.ne=0&select=id");

            yield return requestRoutine;

            if (requestRoutine.Current is DJsonArray dJsonArray)
            {
                callback?.Invoke(dJsonArray.Length > 0);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        public IEnumerator IsHolderOfToken(Action<bool> callback,
            string wallet,
            string contractAddress,
            uint tokenId)
        {
            var requestRoutine =
                GetJson(
                    $"tokens/balances?account={wallet}&token.contract={contractAddress}&token.tokenId={tokenId}&balance.ne=0&select=id");

            yield return requestRoutine;

            if (requestRoutine.Current is DJsonArray dJsonArray)
            {
                callback?.Invoke(dJsonArray.Length > 0);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        public IEnumerator GetTokenMetadata(
            Action<JsonElement> callback,
            string contractAddress,
            uint tokenId)
        {
            var url = $"tokens?contract={contractAddress}&tokenId={tokenId}&select=metadata";
            var requestRoutine = GetJson(url);
            yield return requestRoutine;

            if (requestRoutine.Current is DJsonArray { Length: 1 } dJsonArray)
            {
                // todo: improve this
                var result = JsonSerializer
                    .Deserialize<JsonElement>(dJsonArray.First().ToString(), JsonOptions.DefaultOptions);

                callback?.Invoke(result);
            }
        }

        public IEnumerator GetContractMetadata(
            Action<JsonElement> callback,
            string contractAddress)
        {
            var url = $"contracts/{contractAddress}?legacy=false";
            var requestRoutine = GetJson(url);
            yield return requestRoutine;

            if (requestRoutine.Current is DJsonObject dJsonObject)
            {
                // todo: improve this
                var result = JsonSerializer
                    .Deserialize<JsonElement>(dJsonObject.ToString(), JsonOptions.DefaultOptions);

                callback?.Invoke(result.GetProperty("metadata"));
            }
        }

        public IEnumerator GetTokensForContract(
            Action<IEnumerable<Token>> callback,
            string contractAddress,
            bool withMetadata,
            long maxItems,
            TokensForContractOrder orderBy)
        {
            var sort = orderBy switch
            {
                TokensForContractOrder.Default byDefault =>
                    $"sort.asc=id&offset.cr={byDefault.lastId}",
                TokensForContractOrder.ByLastTimeAsc byLastTimeAsc =>
                    $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
                TokensForContractOrder.ByLastTimeDesc ByLastTimeDesc =>
                    $"sort.desc=lastLevel&offset.pg={ByLastTimeDesc.page}",
                TokensForContractOrder.ByHoldersCountAsc byHoldersCountAsc =>
                    $"sort.asc=holdersCount&offset.pg={byHoldersCountAsc.page}",
                TokensForContractOrder.ByHoldersCountDesc byHoldersCountDesc =>
                    $"sort.desc=holdersCount&offset.pg={byHoldersCountDesc.page}",
                _ => string.Empty
            };

            var url =
                $"tokens?contract={contractAddress}&select=contract,tokenId as token_id" +
                $"{(withMetadata ? ",metadata as token_metadata" : "")},holdersCount as holders_count,id," +
                $"lastTime as last_time&{sort}&limit={maxItems}";

            var requestRoutine = GetJson<IEnumerable<Token>>(url);
            return new CoroutineWrapper<IEnumerable<Token>>(requestRoutine, callback);
        }

        public IEnumerator GetOperationStatus(Action<bool?> callback, string operationHash)
        {
            var url = $"operations/{operationHash}/status";
            var requestRoutine = GetJson<bool?>(url);
            return new CoroutineWrapper<bool?>(requestRoutine, callback, error =>
            {
                Logger.LogDebug($"Can't parse response from API on URL: {url}\n{error.Message}");
            });
        }

        public IEnumerator GetLatestBlockLevel(Action<int> callback)
        {
            var url = $"blocks/{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}/level";
            var requestRoutine = GetJson<int>(url);

            yield return requestRoutine;

            callback?.Invoke(Convert.ToInt32(requestRoutine.Current));
        }
        
        public IEnumerator GetAccountCounter(Action<int> callback, string address)
        {
            var url = $"accounts/{address}/counter";
            var requestRoutine = GetJson<int>(url);
            yield return requestRoutine;
            
            callback?.Invoke(Convert.ToInt32(requestRoutine.Current));
        }

        public IEnumerator GetContractAddressByOperationHash(Action<string> callback, string operationHash)
        {
            var url = $"operations/{operationHash}";
            var requestRoutine = GetJson<IEnumerable<OriginationOperation>>(url);
            yield return new CoroutineWrapper<IEnumerable<OriginationOperation>>(requestRoutine, null, error =>
            {
                Logger.LogDebug(error.Message);
            });

            if (requestRoutine.Current is not IEnumerable<OriginationOperation> operations) yield break;
            var originationOperation = operations.First(op => op.Type == "origination");
            callback.Invoke(originationOperation.Contract.Address);
        }
    }
}