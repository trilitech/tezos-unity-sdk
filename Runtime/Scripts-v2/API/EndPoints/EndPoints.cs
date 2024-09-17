using System;
using System.IO;
using Tezos.Configs;
using Tezos.MessageSystem;

namespace Tezos.API
{
    public static class EndPoints
    {
        private static string _baseUrl;
        
        static EndPoints()
        {
            _baseUrl = ConfigGetter.GetOrCreateConfig<DataProviderConfig>().BaseUrl;
        }
        
        public static string GetBalanceEndPoint(string walletAddress)                           => Path.Combine(_baseUrl, "accounts", walletAddress, "balance");
        public static string GetContractCodeEndPoint(string contract)                           => Path.Combine(_baseUrl, $"chains/main/blocks/head/context/contracts/{contract}/script/");
        public static string GetRunViewEndPoint()                                               => Path.Combine(_baseUrl, "chains/main/blocks/head/helpers/scripts/run_script_view/");
        public static string GetIsHolderOfContractEndPoint(string wallet, string contractAddress) => Path.Combine(_baseUrl, $"tokens/balances?account={wallet}&token.contract={contractAddress}&balance.ne=0&select=id");
        public static string GetIsHolderOfTokenEndPoint(string wallet, string contractAddress, uint tokenId)  => Path.Combine(_baseUrl, $"tokens/balances?account={wallet}&token.contract={contractAddress}&token.tokenId={tokenId}&balance.ne=0&select=id");
        public static string GetTokenMetadataEndPoint(string contractAddress, uint tokenId)  => Path.Combine(_baseUrl, $"tokens?contract={contractAddress}&tokenId={tokenId}&select=metadata");
        public static string GetContractMetadataEndPoint(string contractAddress)  => Path.Combine(_baseUrl, $"accounts/{contractAddress}?legacy=false");
        public static string GetOperationStatusEndPoint(string operationHash)  => Path.Combine(_baseUrl, $"operations/{operationHash}/status");
        public static string GetLatestBlockLevelEndPoint()  => Path.Combine(_baseUrl, $"blocks/{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}/level");
        public static string GetAccountCounterEndPoint(string address)  => Path.Combine(_baseUrl, $"accounts/{address}/counter");
        public static string GetTokensForOwnerEndPoint(string owner, bool withMetadata, long maxItems, string sort)  => Path.Combine(_baseUrl, "tokens/balances?" + $"account={owner}&balance.ne=0&" + "select=account.address as owner,balance,token.contract as token_contract," + $"token.tokenId as token_id{(withMetadata ? ",token.metadata as token_metadata" : "")}," + "lastTime as last_time,id&" + $"{sort}&limit={maxItems}");
        public static string GetOwnersForTokenEndPoint(string contractAddress, uint tokenId, long maxItems, string sort)  => Path.Combine(_baseUrl, "tokens/balances?" + $"token.contract={contractAddress}&balance.ne=0&token.tokenId={tokenId}&" + "select=account.address as owner,balance,token.contract as token_contract," + "token.tokenId as token_id,lastTime as last_time,id&" + $"{sort}&limit={maxItems}");
        public static string GetOwnersForContractEndPoint(string contractAddress, long maxItems, string sort)  => Path.Combine(_baseUrl, "tokens/balances?" + $"token.contract={contractAddress}&balance.ne=0&" + "select=account.address as owner,balance,token.contract as token_contract," + "token.tokenId as token_id,id&" + $"{sort}&limit={maxItems}");
        public static string GetTokensForContractEndPoint(string contractAddress, bool withMetadata, long maxItems, string sort)  => Path.Combine(_baseUrl, $"tokens?contract={contractAddress}&select=contract,tokenId as token_id" + $"{(withMetadata ? ",metadata as token_metadata" : "")},holdersCount as holders_count,id," + $"lastTime as last_time&{sort}&limit={maxItems}");
    }
}