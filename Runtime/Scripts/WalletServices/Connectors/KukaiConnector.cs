
using System;
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
        }
        
        public void Dispose()
        {
        }

        public event Action<WalletMessageType> OperationRequested;

        public void ConnectWallet()
        {
            // Implement logic to redirect user to Kukai embed page
            TezosLog.Debug("ConnectWallet");
            string kukaiUrl = "https://embed-ghostnet.kukai.app?redirect=unitydl://main";
            Application.OpenURL(kukaiUrl);
        }

        public string GetWalletAddress()
        {
            // Implement logic to retrieve the wallet address from the deep link
            TezosLog.Debug("GetWalletAddress");
            return PlayerPrefs.GetString("KukaiWalletAddress");
        }

        public void DisconnectWallet()
        {
            // Implement logic to handle wallet disconnection
            TezosLog.Debug("DisconnectWallet");
            PlayerPrefs.DeleteKey("KukaiWalletAddress");
        }

        public void RequestOperation(WalletOperationRequest operationRequest)
        {
            string operationPayload = $"{{\"destination\":\"{operationRequest.Destination}\",\"amount\":\"{operationRequest.Amount}\",\"entrypoint\":\"{operationRequest.EntryPoint}\",\"arg\":\"{operationRequest.Arg}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?operation={operationPayload}&redirect=unitydl://main";
            TezosLog.Debug("RequestOperation");
            //Application.OpenURL(kukaiUrl);
        }

        public void RequestSignPayload(WalletSignPayloadRequest signRequest)
        {
            string payload = $"{{\"signType\":\"{signRequest.SigningType}\",\"payload\":\"{signRequest.Payload}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?sign={payload}&redirect=unitydl://main";
            TezosLog.Debug("RequestSignPayload");
            //Application.OpenURL(kukaiUrl);
        }

        public void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
        {
            string originationPayload = $"{{\"script\":\"{originationRequest.Script}\",\"delegate\":\"{originationRequest.DelegateAddress}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?originate={originationPayload}&redirect=unitydl://main";
            TezosLog.Debug("RequestSignPayload");
            //Application.OpenURL(kukaiUrl);
        }

    }
}