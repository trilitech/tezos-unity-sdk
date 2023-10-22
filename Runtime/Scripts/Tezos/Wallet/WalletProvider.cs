using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
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
            CoroutineRunner.Instance.StartWrappedCoroutine(InitBeaconConnector());
        }

        private IEnumerator InitBeaconConnector()
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
            yield return Connect(WalletProviderType.beacon, withRedirectToWallet: false);

            // todo: maybe call RequestTezosPermission from _beaconConnector?
            MessageReceiver.PairingCompleted += _ =>
            {
                _beaconConnector.RequestTezosPermission(
                    networkName: TezosConfig.Instance.Network.ToString(),
                    networkRPC: TezosConfig.Instance.RpcBaseUrl);
            };
#endif
            MessageReceiver.HandshakeReceived += handshake =>
            {
                Debug.Log($"Handshake Received: ${handshake}");
                _handshake = handshake;
            };
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

        public void OnReady()
        {
            _beaconConnector.OnReady();
        }

        public IEnumerator Connect(WalletProviderType walletProvider, bool withRedirectToWallet)
        {
            Debug.Log("InitWalletProvider");
            _beaconConnector.InitWalletProvider(
                network: TezosConfig.Instance.Network.ToString(),
                rpc: TezosConfig.Instance.RpcBaseUrl,
                walletProviderType: walletProvider);
            Debug.Log("ConnectAccount");
            var connectAccountCoroutine = _beaconConnector.ConnectAccount().ToCoroutine();
            yield return connectAccountCoroutine;

#if UNITY_ANDROID || UNITY_IOS
            if (withRedirectToWallet){
                Debug.Log("RequestTezosPermission");
                yield return _beaconConnector.RequestTezosPermission(
                    networkName: TezosConfig.Instance.Network.ToString(),
                    networkRPC: TezosConfig.Instance.RpcBaseUrl).ToCoroutine();
                if (string.IsNullOrEmpty(_handshake))
                {
                    //No handshake, Waiting for handshake...
                    Debug.Log("No handshake, Waiting for handshake...");
                    // yield return new WaitUntil(() => string.IsNullOrEmpty(_handshake));
                    yield return new WaitForSecondsRealtime(2.5f);
                }
                if (!string.IsNullOrEmpty(_handshake)){
                    Debug.Log("tezos://?type=tzip10&data=" + _handshake);
                    Application.OpenURL($"tezos://?type=tzip10&data={_handshake}");
                }
            }
#endif
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
