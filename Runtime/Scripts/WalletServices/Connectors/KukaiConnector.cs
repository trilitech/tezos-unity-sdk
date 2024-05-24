
using System;
using System.Threading.Tasks;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using UnityEngine;

namespace TezosSDK.WalletServices.Connectors
{
    public class KukaiConnector : IWalletConnector, IDisposable
    {
        private readonly WalletEventManager _eventManager;

        public KukaiConnector(WalletEventManager eventManager)
        {
            _eventManager = eventManager;
            ConnectorType = ConnectorType.Kukai;
        }
        
        public void Dispose()
        {
        }

        public ConnectorType ConnectorType { get; }
        public event Action<WalletMessageType> OperationRequested;

        public void ConnectWallet()
        {
            // Implement logic to redirect user to Kukai embed page
            TezosLogger.LogDebug("ConnectWallet");
            string kukaiUrl = "https://embed-ghostnet.kukai.app";
            Application.OpenURL(kukaiUrl);
        }

        public string GetWalletAddress()
        {
            // Implement logic to retrieve the wallet address from the deep link
            TezosLogger.LogDebug("GetWalletAddress");
            return "tz1...";
        }

        public void DisconnectWallet()
        {
            // Implement logic to handle wallet disconnection
            TezosLogger.LogDebug("DisconnectWallet");
        }

        public void RequestOperation(WalletOperationRequest operationRequest)
        {
            string operationPayload = $"{{\"destination\":\"{operationRequest.Destination}\",\"amount\":\"{operationRequest.Amount}\",\"entrypoint\":\"{operationRequest.EntryPoint}\",\"arg\":\"{operationRequest.Arg}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?operation={operationPayload}&redirect=unitydl://main";
            TezosLogger.LogDebug("RequestOperation");
            //Application.OpenURL(kukaiUrl);
        }

        public void RequestSignPayload(WalletSignPayloadRequest signRequest)
        {
            string payload = $"{{\"signType\":\"{signRequest.SigningType}\",\"payload\":\"{signRequest.Payload}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?sign={payload}&redirect=unitydl://main";
            TezosLogger.LogDebug("RequestSignPayload");
            //Application.OpenURL(kukaiUrl);
        }

        public void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
        {
            string originationPayload = $"{{\"script\":\"{originationRequest.Script}\",\"delegate\":\"{originationRequest.DelegateAddress}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?originate={originationPayload}&redirect=unitydl://main";
            TezosLogger.LogDebug("RequestSignPayload");
            //Application.OpenURL(kukaiUrl);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}