using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using Scripts.BeaconSDK;
using Scripts.Tezos.API;
using Scripts.Tezos.API.Models.Filters;
using Scripts.Tezos.API.Models.Tokens;
using Scripts.Tezos.Wallet;


namespace Scripts.Tezos
{
    /// <summary>
    /// Tezos API and Wallet features
    /// Exposes the main functions of the Tezos in Unity
    /// </summary>
    public class Tezos : ITezosAPI
    {
        public BeaconMessageReceiver MessageReceiver { get; }
        public ITezosDataAPI API { get; }
        public IWalletProvider Wallet { get; }

        public Tezos()
        {
            var dataProviderConfig = new TzKTProviderConfig();
            API = new TezosDataAPI(dataProviderConfig);
            Wallet = new BeaconWalletProvider();
            
            MessageReceiver = Wallet.MessageReceiver;
        }

        public IEnumerator GetCurrentWalletBalance(Action<ulong> callback)
        {
            var address = Wallet.GetActiveAddress();
            return API.GetTezosBalance(callback, address);
        }

        public void ConnectWallet(WalletProviderType walletProvider, bool withRedirectToWallet = true)
        {
            Wallet.Connect(walletProvider, withRedirectToWallet);
        }

        public void DisconnectWallet()
        {
            Wallet.Disconnect();
        }

        public string GetActiveWalletAddress()
        {
            return Wallet.GetActiveAddress();
        }

        public void RequestSignPayload(SignPayloadType signingType, string payload)
        {
            Wallet.RequestSignPayload(signingType, payload);
        }

        public bool VerifySignedPayload(SignPayloadType signingType, string payload)
        {
            return Wallet.VerifySignedPayload(signingType, payload);
        }

        public void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0)
        {
            Wallet.CallContract(contractAddress, entryPoint, input, amount);
        }

        public IEnumerator ReadBalance(Action<ulong> callback)
        {
            return GetCurrentWalletBalance(callback);
        }

        public IEnumerator ReadView(string contractAddress, string entrypoint, object input,
            Action<JsonElement> callback)
        {
            return API.ReadView(contractAddress, entrypoint, input, callback);
        }

        public IEnumerator GetTokensForOwner(Action<IEnumerable<TokenBalance>> callback, string owner,
            bool withMetadata, long maxItems, TokensForOwnerOrder orderBy)
        {
            return API.GetTokensForOwner(callback, owner, withMetadata, maxItems, orderBy);
        }

        public IEnumerator GetOwnersForToken(Action<IEnumerable<TokenBalance>> callback, string contractAddress,
            uint tokenId, long maxItems, OwnersForTokenOrder orderBy)
        {
            return API.GetOwnersForToken(callback, contractAddress, tokenId, maxItems, orderBy);
        }

        public IEnumerator GetOwnersForContract(Action<IEnumerable<TokenBalance>> callback, string contractAddress,
            long maxItems, OwnersForContractOrder orderBy)
        {
            return API.GetOwnersForContract(callback, contractAddress, maxItems, orderBy);
        }

        public IEnumerator IsHolderOfContract(Action<bool> callback, string wallet, string contractAddress)
        {
            return API.IsHolderOfContract(callback, wallet, contractAddress);
        }

        public IEnumerator IsHolderOfToken(Action<bool> callback, string wallet, string contractAddress, uint tokenId)
        {
            return API.IsHolderOfToken(callback, wallet, contractAddress, tokenId);
        }

        public IEnumerator GetTokenMetadata(Action<JsonElement> callback, string contractAddress, uint tokenId)
        {
            return API.GetTokenMetadata(callback, contractAddress, tokenId);
        }

        public IEnumerator GetContractMetadata(Action<JsonElement> callback, string contractAddress)
        {
            return API.GetContractMetadata(callback, contractAddress);
        }

        public IEnumerator GetTokensForContract(Action<IEnumerable<Token>> callback, string contractAddress,
            bool withMetadata, long maxItems, TokensForContractOrder orderBy)
        {
            return API.GetTokensForContract(callback, contractAddress, withMetadata, maxItems, orderBy);
        }

        public IEnumerator GetOperationStatus(Action<bool?> callback, string operationHash)
        {
            return API.GetOperationStatus(callback, operationHash);
        }
    }
}