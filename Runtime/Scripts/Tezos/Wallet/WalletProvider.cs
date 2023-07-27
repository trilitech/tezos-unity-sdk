using System;
using System.Collections;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Beacon;
using TezosSDK.Helpers;
using UnityEngine;

namespace TezosSDK.Tezos.Wallet
{
    public class WalletProvider : IWalletProvider, IDisposable
    {
        public WalletMessageReceiver MessageReceiver { get; private set; }
        private IBeaconConnector _beaconConnector;

        private string _handshake;
        private string _pubKey;
        private string _signature;
        private string _transactionHash;

        public WalletProvider()
        {
            InitBeaconConnector();
        }

        private void InitBeaconConnector()
        {
            // Create or get a WalletMessageReceiver Game object to receive callback messages
            var unityBeacon = GameObject.Find("UnityBeacon");
            MessageReceiver = unityBeacon != null
                ? unityBeacon.GetComponent<WalletMessageReceiver>()
                : new GameObject("UnityBeacon").AddComponent<WalletMessageReceiver>();

            // Assign the BeaconConnector depending on the platform.
#if !UNITY_EDITOR && UNITY_WEBGL
            _beaconConnector = new BeaconConnectorWebGl();
#else
            _beaconConnector = new BeaconConnectorDotNet();
            (_beaconConnector as BeaconConnectorDotNet)?.SetWalletMessageReceiver(MessageReceiver);
            Connect(WalletProviderType.beacon, withRedirectToWallet: false);

            // todo: maybe call RequestTezosPermission from _beaconConnector?
            MessageReceiver.PairingCompleted += _ =>
            {
                _beaconConnector.RequestTezosPermission(
                    networkName: TezosConfig.Instance.Network.ToString(),
                    networkRPC: TezosConfig.Instance.RpcBaseUrl);
            };
#endif
            MessageReceiver.HandshakeReceived += handshake => { _handshake = handshake; };
            MessageReceiver.AccountConnected += account =>
            {
                var json = JsonSerializer.Deserialize<JsonElement>(account);
                if (!json.TryGetProperty("accountInfo", out json)) return;

                _pubKey = json.GetProperty("publicKey").GetString();
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

                CoroutineRunner.Instance.StartWrappedCoroutine(
                    new CoroutineWrapper<object>(MessageReceiver.TrackTransaction(transactionHash)));
            };
        }

        // Below there are some async/wait workarounds and magic numbers, 
        // we should rewrite the Beacon connector to be coroutine compatible.
        private IEnumerator OnOpenWallet(bool withRedirectToWallet)
        {
            var address = _beaconConnector.GetActiveAccountAddress();
            if (!string.IsNullOrEmpty(address))
            {
                // Active address, let's disconnect it
                _beaconConnector.DisconnectAccount();
                yield return new WaitForSeconds(2.5f);
            }
            if (string.IsNullOrEmpty(_handshake))
            {
                //No handshake, Waiting for handshake...
                yield return new WaitForSeconds(2.5f);
            }

#if UNITY_ANDROID || UNITY_IOS
            if (withRedirectToWallet){
                _beaconConnector.RequestTezosPermission(
                    networkName: TezosConfig.Instance.Network.ToString(),
                    networkRPC: TezosConfig.Instance.RpcBaseUrl);
                yield return new WaitForSeconds(2.5f);
                if (!string.IsNullOrEmpty(_handshake)){
                    Application.OpenURL($"tezos://?type=tzip10&data={_handshake}");
                }
            }
#endif
        }

        public void Connect(WalletProviderType walletProvider, bool withRedirectToWallet)
        {
            _beaconConnector.InitWalletProvider(
                network: TezosConfig.Instance.Network.ToString(),
                rpc: TezosConfig.Instance.RpcBaseUrl,
                walletProviderType: walletProvider);
            _beaconConnector.ConnectAccount();
            CoroutineRunner.Instance.StartWrappedCoroutine(OnOpenWallet(withRedirectToWallet));
        }

        public void Disconnect()
        {
            _beaconConnector.DisconnectAccount();
        }

        public string GetActiveAddress()
        {
            return _beaconConnector.GetActiveAccountAddress();
        }


        public void RequestSignPayload(SignPayloadType signingType, string payload)
        {
            _beaconConnector.RequestTezosSignPayload(signingType, payload);
        }

        public bool VerifySignedPayload(SignPayloadType signingType, string payload)
        {
            return NetezosExtensions.VerifySignature(_pubKey, signingType, payload, _signature);
        }

        public void CallContract(
            string contractAddress,
            string entryPoint,
            string input,
            ulong amount = 0)
        {
            _beaconConnector.RequestTezosOperation(
                destination: contractAddress,
                entryPoint: entryPoint,
                arg: input,
                amount: amount,
                networkName: TezosConfig.Instance.Network.ToString(),
                networkRPC: TezosConfig.Instance.RpcBaseUrl);
        }

        public void OriginateContract(
            string script,
            string delegateAddress)
        {
            _beaconConnector.RequestContractOrigination(script, delegateAddress);
        }

        public void Dispose()
        {
            if (_beaconConnector is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
