using System;
using System.Collections;
using System.Linq;
using System.Text.Json;
using TezosSDK.Tezos;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Beacon
{
    /// <summary>
    /// Handles wallet-related events, providing a way to subscribe to different wallet actions.
    /// </summary>
    public class WalletMessageReceiver : MonoBehaviour
    {
        // Events are only added if they are not already in the invocation list.
        // Each event corresponds to a specific wallet action and returns a string result.

        private Action<string> _accountConnected;
        private Action<string> _accountConnectionFailed;
        private Action<string> _accountDisconnected;
        private Action<string> _contractCallCompleted;
        private Action<string> _contractCallInjected;
        private Action<string> _contractCallFailed;
        private Action<string> _payloadSigned;
        private Action<string> _handshakeReceived;
        private Action<string> _pairingCompleted;

        /// <summary>
        /// Triggered when an account is successfully connected.
        /// The returned string contains JSON-formatted account information.
        /// </summary>
        public event Action<string> AccountConnected
        {
            add
            {
                if (_accountConnected == null || !_accountConnected.GetInvocationList().Contains(value))
                    _accountConnected += value;
            }
            remove => _accountConnected -= value;
        }

        /// <summary>
        /// Triggered when an account connection attempt fails.
        /// The returned string contains JSON-formatted account information.
        /// </summary>
        public event Action<string> AccountConnectionFailed
        {
            add
            {
                if (_accountConnectionFailed == null || !_accountConnectionFailed.GetInvocationList().Contains(value))
                    _accountConnectionFailed += value;
            }
            remove => _accountConnectionFailed -= value;
        }

        /// <summary>
        /// Triggered when an account is disconnected.
        /// The returned string is empty. TODO: Add JSON-formatted account information as return string.
        /// </summary>
        public event Action<string> AccountDisconnected
        {
            add
            {
                if (_accountDisconnected == null || !_accountDisconnected.GetInvocationList().Contains(value))
                    _accountDisconnected += value;
            }
            remove => _accountDisconnected -= value;
        }

        /// <summary>
        /// Triggered when a contract call is completed.
        /// The returned string is a JSON-formatted object of type ContractCallInjectionResult
        /// </summary>
        public event Action<string> ContractCallCompleted
        {
            add
            {
                if (_contractCallCompleted == null || !_contractCallCompleted.GetInvocationList().Contains(value))
                    _contractCallCompleted += value;
            }
            remove => _contractCallCompleted -= value;
        }


        /// <summary>
        /// Triggered when a contract call is injected into the blockchain.
        /// The returned string is a JSON-formatted object of type ContractCallInjectionResult
        /// </summary>
        public event Action<string> ContractCallInjected
        {
            add
            {
                if (_contractCallInjected == null || !_contractCallInjected.GetInvocationList().Contains(value))
                    _contractCallInjected += value;
            }
            remove => _contractCallInjected -= value;
        }


        /// <summary>
        /// Triggered when a contract call fails.
        /// The returned string is a JSON-formatted object of type ContractCallInjectionResult
        /// TODO: "result is error or empty" - please clarify the error type and format and if empty on success
        /// </summary>
        public event Action<string> ContractCallFailed
        {
            add
            {
                if (_contractCallFailed == null || !_contractCallFailed.GetInvocationList().Contains(value))
                    _contractCallFailed += value;
            }
            remove => _contractCallFailed -= value;
        }

        /// <summary>
        /// Triggered when a payload is signed.
        /// The returned string is a JSON string of the payload signing result.
        /// </summary>
        public event Action<string> PayloadSigned
        {
            add
            {
                if (_payloadSigned == null || !_payloadSigned.GetInvocationList().Contains(value))
                    _payloadSigned += value;
            }
            remove => _payloadSigned -= value;
        }

        /// <summary>
        /// Triggered when a handshake is received.
        /// The returned string is a serialized summary of the pairing request,
        /// including app details like name, relay servers, and URLs.
        /// </summary>
        public event Action<string> HandshakeReceived
        {
            add
            {
                if (_handshakeReceived == null || !_handshakeReceived.GetInvocationList().Contains(value))
                    _handshakeReceived += value;
            }
            remove => _handshakeReceived -= value;
        }

        /// <summary>
        /// Invoked when the pairing process with a DApp is successfully completed.
        /// The returned string is a JSON-formatted object containing key details about the pairing, 
        /// including the DApp's name, URL, the network type, RPC URL, and the timestamp of the pairing completion.
        /// </summary>
        public event Action<string> PairingCompleted
        {
            add
            {
                if (_pairingCompleted == null || !_pairingCompleted.GetInvocationList().Contains(value))
                    _pairingCompleted += value;
            }
            remove => _pairingCompleted -= value;
        }


        public void OnAccountConnected(string address)
        {
            // result is the json permission response
            Logger.LogDebug("From unity, OnAccountConnected: " + address);
            _accountConnected?.Invoke(address);
        }

        public void OnAccountFailedToConnect(string result)
        {
            // result is the json error
            Logger.LogDebug("From unity, OnAccountFailedToConnect: " + result);
            _accountConnectionFailed?.Invoke(result);
        }

        public void OnAccountDisconnected(string result)
        {
            Logger.LogDebug("From unity, OnAccountDisconnect: " + result);
            _accountDisconnected?.Invoke(result);
        }

        public void OnContractCallCompleted(string result)
        {
            // result is the json of transaction response
            Logger.LogDebug("From unity, OnContractCallCompleted: " + result);
            _contractCallCompleted?.Invoke(result);
        }

        public void OnContractCallInjected(string result)
        {
            // result is the json of transaction response
            Logger.LogDebug("From unity, OnContractCallInjected: " + result);
            _contractCallInjected?.Invoke(result);
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
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
                Logger.LogDebug($"Checking tx status: {transactionHash}");
                
                
                yield return TezosManager
                    .Instance
                    .Tezos
                    .API
                    .GetOperationStatus(result =>
                    {
                        if (result != null)
                            success = JsonSerializer.Deserialize<bool>(result);
                    }, transactionHash);

                yield return new WaitForSecondsRealtime(3);
            }

            ContractCallInjectionResult result;
            result.success = success;
            result.transactionHash = transactionHash;
            _contractCallCompleted?.Invoke(JsonUtility.ToJson(result));
        }

        public void OnContractCallFailed(string result)
        {
            // result is error or empty
            Logger.LogDebug("From unity, OnContractCallFailed: " + result);
            _contractCallFailed?.Invoke(result);
        }

        public void OnPayloadSigned(string signature)
        {
            // result is the json string of payload signing result
            Logger.LogDebug("From unity, OnPayloadSigned: " + signature);
            _payloadSigned?.Invoke(signature);
        }

        public void OnHandshakeReceived(string handshake)
        {
            // result is serialized p2p pairing request
            Logger.LogDebug("From unity, OnHandshakeReceived: " + handshake);
            _handshakeReceived?.Invoke(handshake);
        }

        public void OnPairingCompleted(string message)
        {
            Logger.LogDebug("From unity, OnPairingCompleted: " + message);
            _pairingCompleted?.Invoke(message);
        }
    }
}