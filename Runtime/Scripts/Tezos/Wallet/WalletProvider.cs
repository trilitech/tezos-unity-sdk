using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;
using Scripts.BeaconSDK;
using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Tezos.Wallet
{
    public class WalletProvider : IWalletProvider
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
            // Create a WalletMessageReceiver Game object to receive callback messages
            MessageReceiver = new GameObject("UnityBeacon").AddComponent<WalletMessageReceiver>();

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

        public void Connect(WalletProviderType walletProvider, bool withRedirectToWallet)
        {
            _beaconConnector.InitWalletProvider(
                network: TezosConfig.Instance.Network.ToString(),
                rpc: TezosConfig.Instance.RpcBaseUrl,
                walletProviderType: walletProvider);

            _beaconConnector.ConnectAccount();
#if UNITY_ANDROID || UNITY_IOS
            if (withRedirectToWallet)
                Application.OpenURL($"tezos://?type=tzip10&data={_handshake}");
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
    }
}