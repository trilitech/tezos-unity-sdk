using System;
using System.Collections;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Beacon;
using TezosSDK.Helpers;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos.Wallet
{
    public class WalletProvider : IWalletProvider, IDisposable
    {
        public WalletEventManager EventManager { get; private set; }
        private IBeaconConnector _beaconConnector;
        private DAppMetadata _dAppMetadata;

        private string _handshake;
        private string _pubKey;
        private string _signature;
        private string _transactionHash;

        public bool IsConnected { get; private set; }

        public WalletProvider(DAppMetadata dAppMetadata)
        {
            _dAppMetadata = dAppMetadata;
            InitBeaconConnector();
        }

        private void InitBeaconConnector()
        {
            // Get a WalletEventManager instance to receive callback messages
            EventManager = WalletEventManager.Instance;
            
            // Assign the BeaconConnector depending on the platform.
#if !UNITY_EDITOR && UNITY_WEBGL
            _beaconConnector = new BeaconConnectorWebGl();
#else
            _beaconConnector = new BeaconConnectorDotNet();
            (_beaconConnector as BeaconConnectorDotNet)?.SetWalletMessageReceiver(EventManager);
            Connect(WalletProviderType.beacon, withRedirectToWallet: false);

            // todo: maybe call RequestTezosPermission from _beaconConnector?
            EventManager.PairingCompleted += _ =>
            {
                _beaconConnector.RequestTezosPermission(
                    networkName: TezosConfig.Instance.Network.ToString(),
                    networkRPC: TezosConfig.Instance.RpcBaseUrl);
            };
#endif
            EventManager.HandshakeReceived += handshake => { _handshake = handshake.PairingData; };

            EventManager.AccountConnected += account => { _pubKey = account.PublicKey; };

            EventManager.PayloadSigned += payload => { _signature = payload.Signature; };

            EventManager.ContractCallInjected += transaction =>
            {
                var transactionHash = transaction.TransactionHash;

                CoroutineRunner.Instance.StartWrappedCoroutine(
                    new CoroutineWrapper<object>(TrackTransaction(transactionHash)));
            };
        }

        // TODO: Find a better place for this, used to be in WalletMessageReceiver
        private IEnumerator TrackTransaction(string transactionHash)
        {
            var success = false;
            const float timeout = 30f; // seconds
            var timestamp = Time.time;

            // keep making requests until time out or success
            while (!success && Time.time - timestamp < timeout)
            {
                Logger.LogDebug($"Checking tx status: {transactionHash}");

                yield return TezosManager
                    .Instance
                    .Tezos
                    .API
                    .GetOperationStatus(result =>
                    {
                        if (result != null)
                        {
                            success = JsonSerializer.Deserialize<bool>(result);
                        }
                    }, transactionHash);

                yield return new WaitForSecondsRealtime(3);
            }

            var operationResult = new OperationResult
            {
                TransactionHash = transactionHash
            };

            var contractCallCompletedEvent = new UnifiedEvent
            {
                EventType = WalletEventManager.EventTypeContractCallCompleted,
                Data = JsonUtility.ToJson(operationResult)
            };

            EventManager.HandleEvent(JsonUtility.ToJson(contractCallCompletedEvent));
        }


        // Below there are some async/wait workarounds and magic numbers, 
        // we should rewrite the Beacon connector to be coroutine compatible.
        private IEnumerator OnOpenWallet(bool withRedirectToWallet)
        {
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

        public void OnReady()
        {
            _beaconConnector.OnReady();
        }

        public void Connect(WalletProviderType walletProvider, bool withRedirectToWallet)
        {
            _beaconConnector.InitWalletProvider(
                network: TezosConfig.Instance.Network.ToString(),
                rpc: TezosConfig.Instance.RpcBaseUrl,
                walletProviderType: walletProvider,
                dAppMetadata: _dAppMetadata);

            _beaconConnector.ConnectAccount();
            CoroutineRunner.Instance.StartWrappedCoroutine(OnOpenWallet(withRedirectToWallet));
            IsConnected = true;
        }

        public void Disconnect()
        {
            _beaconConnector.DisconnectAccount();
            IsConnected = false;
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