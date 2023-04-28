using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;
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
        private string _networkName;
        private string _indexerNode;
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
            string tzKTApi = "https://api.tzkt.io/v1/") : base(tzKTApi)
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
			_beaconConnector.SetNetwork(_networkName, NetworkRPC);
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
            MessageReceiver.PayloadSigned += (payload) =>
            {
                var json = JsonSerializer.Deserialize<JsonElement>(payload);
                var signature = json.GetProperty("signature").GetString();
                _signature = signature;
            };
            MessageReceiver.ContractCallInjected += transaction =>
            {
                var json = JsonSerializer.Deserialize<JsonElement>(transaction);
                var transactionHash = json.GetProperty("transactionHash").GetString();
                CoroutineUtils.TryWith(MessageReceiver,
                    MessageReceiver.ContractCallInjection(_indexerNode, transactionHash),
                    (ex) =>
                    {
                        if (ex != null)
                            Debug.Log("An exception related to Tezos ContractCallInjection: " + ex);
                    });
            };
        }

        public void ConnectWallet()
        {
#if UNITY_WEBGL
            _beaconConnector.ConnectAccount();
#elif UNITY_ANDROID || UNITY_IOS
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

        public void RequestSignPayload(SignPayloadType signingType, string payload)
        {
            _beaconConnector.RequestTezosSignPayload(signingType, payload);
        }

        public bool VerifySignedPayload(SignPayloadType signingType, string payload)
        {
            var key = _pubKey;
            var signature = _signature;
            return NetezosExtensions.VerifySignature(key, signingType, payload, signature);
        }

        public IEnumerator GetTokensForOwner(
            Action<IEnumerable<TokenBalance>> cb,
            string owner,
            bool withMetadata,
            long maxItems,
            TokensForOwnerOrder orderBy
        )
        {
            var sort = orderBy switch
            {
                TokensForOwnerOrder.Default byDefault => $"sort.asc=id&offset.cr={byDefault.lastId}",
                TokensForOwnerOrder.ByLastTimeAsc byLastTimeAsc => $"sort.asc=lastLevel&offset.pg={byLastTimeAsc.page}",
                TokensForOwnerOrder.ByLastTimeDesc ByLastTimeDesc => $"sort.desc=lastLevel&offset.pg={ByLastTimeDesc.page}",
                _ => string.Empty
            };

            var url = "tokens/balances?" +
                      $"account={owner}&" +
                      "balance.ne=0&" +
                      "select=account.address as owner,balance,token.contract as token_contract," +
                      "token.tokenId as token_id,token.metadata as token_metadata,lastTime as last_time,id&" +
                      $"{sort}&" +
                      $"limit={maxItems}";

            var requestRoutine = GetJson<IEnumerable<TokenBalance>>(url);
            return WrappedRequest(requestRoutine, cb);
        }
    }
}