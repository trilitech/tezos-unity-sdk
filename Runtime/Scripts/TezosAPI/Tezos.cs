using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BeaconSDK;
using Dynamic.Json;
using Helpers;
using TezosAPI.Models;
using TezosAPI.Models.Tokens;
using UnityEngine;

namespace TezosAPI
{
    /// <summary>
    /// Implementation of the ITezosAPI.
    /// Exposes the main functions of the Tezos API in Unity
    /// </summary>
    public class Tezos : HttpClient, ITezosAPI
    {
        private readonly string _networkName;
        private readonly string _indexerNode;
        private IBeaconConnector _beaconConnector;

        private string _handshake;
        private string _pubKey;
        private string _signature;
        private string _transactionHash;

        public string NetworkRPC { get; private set; }

        public BeaconMessageReceiver MessageReceiver { get; private set; }

        public Tezos(
            string networkName = "ghostnet",
            string networkRPC = "https://rpc.ghostnet.teztnets.xyz",
            string indexerNode = "https://api.ghostnet.tzkt.io/v1/operations/{0}/status",
            string tzKTApi = "https://api.ghostnet.tzkt.io/v1/") : base(tzKTApi)
        {
            _networkName = networkName;
            _indexerNode = indexerNode;
            NetworkRPC = networkRPC;

            InitBeaconConnector();
        }

        private void InitBeaconConnector()
        {
            // Create a BeaconMessageReceiver Game object to receive callback messages
            MessageReceiver = new GameObject("UnityBeacon").AddComponent<BeaconMessageReceiver>();

            // Assign the BeaconConnector depending on the platform.
#if UNITY_WEBGL && !UNITY_EDITOR
			_beaconConnector = new BeaconConnectorWebGl();
			_beaconConnector.SetNetwork(_networkName, NetworkRPC);;
#elif (UNITY_ANDROID && !UNITY_EDITOR) || (UNITY_IOS && !UNITY_EDITOR) || UNITY_STANDALONE || UNITY_EDITOR
            _beaconConnector = new BeaconConnectorDotNet();
            _beaconConnector.SetNetwork(_networkName, NetworkRPC);
            (_beaconConnector as BeaconConnectorDotNet)?.SetBeaconMessageReceiver(MessageReceiver);
            _beaconConnector.ConnectAccount();
            MessageReceiver.PairingCompleted += _ => RequestPermission();
#else
            _beaconConnector = new BeaconConnectorNull();
#endif

            MessageReceiver.ClientCreated += _ => { _beaconConnector.RequestHandshake(); };
            MessageReceiver.HandshakeReceived += handshake => { _handshake = handshake; };

            MessageReceiver.AccountConnected += account =>
            {
                var json = JsonSerializer.Deserialize<JsonElement>(account);
                if (json.TryGetProperty("account", out json))
                {
                    _pubKey = json.GetProperty("publicKey").GetString();

                    Debug.Log("my pubkey: " + _pubKey);
                }
            };
            MessageReceiver.PayloadSigned += payload =>
            {
                var json = JsonSerializer.Deserialize<JsonElement>(payload);
                var signature = json.GetProperty("signature").GetString();
                _signature = signature;
            };
            MessageReceiver.ContractCallInjected += transaction =>
            {
                var json = JsonSerializer.Deserialize<JsonElement>(transaction);
                var transactionHash = json.GetProperty("transactionHash").GetString();
                MessageReceiver.StartCoroutine(MessageReceiver.ContractCallInjection(_indexerNode, transactionHash));
            };
        }

        public void ConnectWallet()
        {
#if UNITY_WEBGL
            _beaconConnector.ConnectAccount();
#elif UNITY_ANDROID || UNITY_IOS
            RequestPermission();
            Application.OpenURL($"tezos://?type=tzip10&data={_handshake}");
#endif
        }

        public void DisconnectWallet()
        {
            _beaconConnector.DisconnectAccount();
        }

        public string GetActiveWalletAddress()
        {
            return _beaconConnector.GetActiveAccountAddress();
        }

        public IEnumerator ReadBalance(Action<ulong> callback)
        {
            var address = _beaconConnector.GetActiveAccountAddress();
            return NetezosExtensions.ReadTZBalance(NetworkRPC, address, callback);
        }

        public IEnumerator ReadView(string contractAddress, string entryPoint, object input,
            Action<JsonElement> callback)
        {
            return NetezosExtensions.ReadView(NetworkRPC, contractAddress, entryPoint, input, callback);
        }

        public void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0)
        {
            _beaconConnector.RequestTezosOperation(contractAddress, entryPoint, input,
                amount, _networkName, NetworkRPC);
        }

        public void RequestPermission()
        {
            _beaconConnector.RequestTezosPermission(_networkName, NetworkRPC);
        }

        public void RequestSignPayload(int signingType, string payload)
        {
            _beaconConnector.RequestTezosSignPayload(signingType, payload);
        }

        public bool VerifySignedPayload(string payload)
        {
            var key = _pubKey;
            var signature = _signature;
            return NetezosExtensions.VerifySignature(key, payload, signature);
        }

        public IEnumerator GetTokensForOwner(
            Action<IEnumerable<TokenBalance>> cb,
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
            return WrappedRequest(requestRoutine, cb);
        }

        public IEnumerator GetOwnersForToken(
            Action<IEnumerable<TokenBalance>> cb,
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
            return WrappedRequest(requestRoutine, cb);
        }

        public IEnumerator GetOwnersForContract(
            Action<IEnumerable<TokenBalance>> cb,
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
            return WrappedRequest(requestRoutine, cb);
        }

        public IEnumerator IsHolderOfContract(
            Action<bool> cb,
            string wallet,
            string contractAddress)
        {
            var requestRoutine =
                GetJson($"tokens/balances?account={wallet}&token.contract={contractAddress}&balance.ne=0&select=id");

            yield return requestRoutine;

            if (requestRoutine.Current is DJsonArray dJsonArray)
            {
                cb?.Invoke(dJsonArray.Length > 0);
            }
            else
            {
                cb?.Invoke(false);
            }
        }

        public IEnumerator GetTokenMetadata(
            Action<JsonElement> cb,
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

                cb?.Invoke(result);
            }
        }

        public IEnumerator GetContractMetadata(
            Action<JsonElement> cb,
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

                cb?.Invoke(result.GetProperty("metadata"));
            }
        }
    }
}