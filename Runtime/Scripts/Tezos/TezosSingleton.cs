using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Beacon.Sign;
using Scripts.BeaconSDK;
using Scripts.Helpers;
using Scripts.Tezos.API;
using Scripts.Tezos.API.Models.Filters;
using Scripts.Tezos.API.Models.Tokens;
using Scripts.Tezos.Wallet;


namespace Scripts.Tezos
{
    public class TezosSingleton : SingletonMonoBehaviour<TezosSingleton>, ITezosAPI
    {
        private static Tezos _tezos;
        public BeaconMessageReceiver MessageReceiver => _tezos.MessageReceiver;
        public ITezosDataAPI API => _tezos.API;
        public IWalletProvider Wallet => _tezos.Wallet;

        protected override void Awake()
        {
            base.Awake();

            Logger.CurrentLogLevel = Logger.LogLevel.Debug;
            TezosConfig.Instance.Network = NetworkType.ghostnet;
            _tezos = new Tezos();
        }

        public IEnumerator GetCurrentWalletBalance(Action<ulong> callback)
        {
            return _tezos.GetCurrentWalletBalance(callback);
        }

        public void ConnectWallet(bool withRedirectToWallet = true)
        {
            _tezos.ConnectWallet(withRedirectToWallet);
        }

        public void DisconnectWallet()
        {
            _tezos.DisconnectWallet();
        }

        public string GetActiveWalletAddress()
        {
            return _tezos.GetActiveWalletAddress();
        }

        public void RequestSignPayload(SignPayloadType signingType, string payload)
        {
            _tezos.RequestSignPayload(signingType, payload);
        }

        public bool VerifySignedPayload(SignPayloadType signingType, string payload)
        {
            return _tezos.VerifySignedPayload(signingType, payload);
        }

        public void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0)
        {
            _tezos.CallContract(contractAddress, entryPoint, input, amount);
        }

        public IEnumerator ReadBalance(Action<ulong> callback)
        {
            return _tezos.ReadBalance(callback);
        }

        public IEnumerator ReadView(string contractAddress, string entrypoint, object input,
            Action<JsonElement> callback)
        {
            return _tezos.ReadView(contractAddress, entrypoint, input, callback);
        }

        public IEnumerator GetTokensForOwner(Action<IEnumerable<TokenBalance>> callback, string owner,
            bool withMetadata, long maxItems, TokensForOwnerOrder orderBy)
        {
            return _tezos.GetTokensForOwner(callback, owner, withMetadata, maxItems, orderBy);
        }

        public IEnumerator GetOwnersForToken(Action<IEnumerable<TokenBalance>> callback, string contractAddress,
            uint tokenId, long maxItems, OwnersForTokenOrder orderBy)
        {
            return _tezos.GetOwnersForToken(callback, contractAddress, tokenId, maxItems, orderBy);
        }

        public IEnumerator GetOwnersForContract(Action<IEnumerable<TokenBalance>> callback, string contractAddress,
            long maxItems, OwnersForContractOrder orderBy)
        {
            return _tezos.GetOwnersForContract(callback, contractAddress, maxItems, orderBy);
        }

        public IEnumerator IsHolderOfContract(Action<bool> callback, string wallet, string contractAddress)
        {
            return _tezos.IsHolderOfContract(callback, wallet, contractAddress);
        }

        public IEnumerator IsHolderOfToken(Action<bool> callback, string wallet, string contractAddress, uint tokenId)
        {
            return _tezos.IsHolderOfToken(callback, wallet, contractAddress, tokenId);
        }

        public IEnumerator GetTokenMetadata(Action<JsonElement> callback, string contractAddress, uint tokenId)
        {
            return _tezos.GetTokenMetadata(callback, contractAddress, tokenId);
        }

        public IEnumerator GetContractMetadata(Action<JsonElement> callback, string contractAddress)
        {
            return _tezos.GetContractMetadata(callback, contractAddress);
        }

        public IEnumerator GetTokensForContract(Action<IEnumerable<Token>> callback, string contractAddress,
            bool withMetadata, long maxItems, TokensForContractOrder orderBy)
        {
            return _tezos.GetTokensForContract(callback, contractAddress, withMetadata, maxItems, orderBy);
        }

        public IEnumerator GetOperationStatus(Action<bool?> callback, string operationHash)
        {
            return _tezos.GetOperationStatus(callback, operationHash);
        }
    }
}