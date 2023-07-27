using System;
using System.Collections.Generic;
using System.Text.Json;
using TezosSDK.Helpers;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using UnityEngine;

namespace TezosSDK.Samples.NFTApiSample
{
    public class DataManager : MonoBehaviour
    {
        private ITezos _tezos;
        private string _connectedAddress;
        private string _checkContract;
        private string _checkAddress;
        private string _checkTokenId;

        public Action<string> DataReceived;

        private const int MaxTokens = 20;

        void Start()
        {
            _tezos = TezosSingleton.Instance;
            _tezos
                .Wallet
                .MessageReceiver
                .AccountConnected += OnAccountConnected;
        }

        void OnAccountConnected(string result)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(result);
            var account = json.GetProperty("accountInfo");
            _connectedAddress = account.GetProperty("address").GetString();
        }

        public void GetTokensForOwners()
        {
            var walletAddress = string.IsNullOrEmpty(_checkAddress)
                ? _connectedAddress
                : _checkAddress;

            CoroutineRunner.Instance.StartCoroutine(
                _tezos.API.GetTokensForOwner((tbs) =>
                    {
                        if (tbs == null)
                        {
                            DataReceived.Invoke($"Incorrect address - {walletAddress}");
                            Debug.Log($"Incorrect address - {walletAddress}");
                            return;
                        }

                        List<TokenBalance> tokens = new List<TokenBalance>(tbs);
                        if (tokens.Count > 0)
                        {
                            var result = "";
                            foreach (var tb in tokens)
                            {
                                result +=
                                    $"{walletAddress} has {tb.Balance} tokens on contract {tb.TokenContract.Address}" +
                                    "\r\n" + "\r\n";
                                Debug.Log(
                                    $"{walletAddress} has {tb.Balance} tokens on contract {tb.TokenContract.Address}");
                            }

                            DataReceived.Invoke(result);
                        }
                        else
                        {
                            DataReceived.Invoke($"{walletAddress} has no tokens");
                            Debug.Log($"{walletAddress} has no tokens");
                        }
                    },
                    owner: walletAddress,
                    withMetadata: false,
                    maxItems: MaxTokens,
                    orderBy: new TokensForOwnerOrder.Default(0)));
        }

        public void IsHolderOfContract()
        {
            var walletAddress = string.IsNullOrEmpty(_checkAddress)
                ? _connectedAddress
                : _checkAddress;

            if (string.IsNullOrEmpty(_checkContract))
            {
                DataReceived.Invoke("Enter contract address");
                Debug.Log("Enter contract address");
                return;
            }
            
            CoroutineRunner.Instance.StartCoroutine(_tezos.API.IsHolderOfContract((flag) =>
                {
                    var message = flag
                        ? $"{walletAddress} is HOLDER of contract {_checkContract}"
                        : $"{walletAddress} is NOT HOLDER of contract {_checkContract}";

                    DataReceived.Invoke(message);
                    Debug.Log(message);
                },
                wallet: walletAddress,
                contractAddress: _checkContract));
        }

        public void IsHolderOfToken()
        {
            var walletAddress = string.IsNullOrEmpty(_checkAddress)
                ? _connectedAddress
                : _checkAddress;

            var tokenId = string.IsNullOrEmpty(_checkTokenId)
                ? 0
                : Convert.ToUInt32(_checkTokenId);

            if (string.IsNullOrEmpty(_checkContract))
            {
                DataReceived.Invoke("Enter contract address");
                Debug.Log("Enter contract address");
                return;
            }
            
            CoroutineRunner.Instance.StartCoroutine(_tezos.API.IsHolderOfToken((flag) =>
                {
                    var message = flag
                        ? $"{walletAddress} is HOLDER of token"
                        : $"{walletAddress} is NOT HOLDER of token";

                    DataReceived.Invoke(message);
                    Debug.Log(message);
                },
                wallet: walletAddress,
                contractAddress: _checkContract,
                tokenId: tokenId));
        }

        public void SetCheckAddress(string address)
        {
            _checkAddress = address;
        }

        public void SetCheckTokenId(string tokenId)
        {
            _checkTokenId = tokenId;
        }

        public void SetCheckContract(string contract)
        {
            _checkContract = contract;
        }
    }
}