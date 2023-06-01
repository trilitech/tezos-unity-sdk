using System;
using System.Collections;
using System.Text.Json;
using Scripts.Tezos;
using UnityEngine;
using Logger = Scripts.Helpers.Logger;

namespace Scripts.BeaconSDK
{
    /// <summary>
    /// Receives external messages
    /// </summary>
    public class WalletMessageReceiver : BeaconMessageReceiver
    {
    }

    /// <summary>
    /// Receives external messages
    /// </summary>
    [Obsolete(
        "BeaconMessageReceiver will be renamed to WalletMessageReceiver in future versions, please use WalletMessageReceiver type instead")]
    public class BeaconMessageReceiver : MonoBehaviour
    {
        public event Action<string> AccountConnected;
        public event Action<string> AccountConnectionFailed;
        public event Action<string> AccountDisconnected;
        public event Action<string> ContractCallCompleted;
        public event Action<string> ContractCallInjected;
        public event Action<string> ContractCallFailed;
        public event Action<string> PayloadSigned;
        public event Action<string> HandshakeReceived;
        public event Action<string> PairingCompleted;


        public void OnAccountConnected(string address)
        {
            // result is the json permission response
            Logger.LogDebug("From unity, OnAccountConnected: " + address);
            AccountConnected?.Invoke(address);
        }

        public void OnAccountFailedToConnect(string result)
        {
            // result is the json error
            Logger.LogDebug("From unity, OnAccountFailedToConnect: " + result);
            AccountConnectionFailed?.Invoke(result);
        }

        public void OnAccountDisconnected(string result)
        {
            Logger.LogDebug("From unity, OnAccountDisconnect: " + result);
            AccountDisconnected?.Invoke(result);
        }

        public void OnContractCallCompleted(string result)
        {
            // result is the json of transaction response
            Logger.LogDebug("From unity, OnContractCallCompleted: " + result);
            ContractCallCompleted?.Invoke(result);
        }

        public void OnContractCallInjected(string result)
        {
            // result is the json of transaction response
            Logger.LogDebug("From unity, OnContractCallInjected: " + result);
            ContractCallInjected?.Invoke(result);
        }

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        [Serializable]
        struct ContractCallInjectionResult
        {
            public bool success;
            public string transactionHash;
        }

        public IEnumerator TrackTransaction(string transactionHash)
        {
            var success = false;
            const float timeout = 30f; // seconds
            var timestamp = Time.time;

            // keep making requests until time out or success
            while (!success && Time.time - timestamp < timeout)
            {
                if (success) break;
                Logger.LogDebug($"Checking tx status: {transactionHash}");
                yield return TezosSingleton.Instance.GetOperationStatus(result =>
                {
                    if (result != null)
                        success = JsonSerializer.Deserialize<bool>(result);
                }, transactionHash);

                yield return new WaitForSecondsRealtime(3);
            }

            ContractCallInjectionResult result;
            result.success = success;
            result.transactionHash = transactionHash;
            ContractCallCompleted?.Invoke(JsonUtility.ToJson(result));
        }

        public void OnContractCallFailed(string result)
        {
            // result is error or empty
            Logger.LogDebug("From unity, OnContractCallFailed: " + result);
            ContractCallFailed?.Invoke(result);
        }

        public void OnPayloadSigned(string signature)
        {
            // result is the json string of payload signing result
            Logger.LogDebug("From unity, OnPayloadSigned: " + signature);
            PayloadSigned?.Invoke(signature);

        }

        public void OnHandshakeReceived(string handshake)
        {
            // result is serialized p2p pairing request
            Logger.LogDebug("From unity, OnHandshakeReceived: " + handshake);
            HandshakeReceived?.Invoke(handshake);
        }

        public void OnPairingCompleted(string message)
        {
            Logger.LogDebug("From unity, OnPairingCompleted: " + message);
            PairingCompleted?.Invoke(message);
        }
    }
}