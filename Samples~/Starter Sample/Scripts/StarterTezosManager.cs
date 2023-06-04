using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;
using Scripts.BeaconSDK;
using Scripts.Helpers;
using Scripts.Tezos;
using Scripts.Tezos.API;
using Scripts.Tezos.Wallet;
using Logger = Scripts.Helpers.Logger;

public class StarterTezosManager : MonoBehaviour
{
    public WalletMessageReceiver MessageReceiver { get; private set; }
    public ITezosDataAPI API { get; private set; }
    //public IWalletProvider Wallet { get; private set; }
    public IBeaconConnector BeaconConnector { get; private set; }
    
    public static StarterTezosManager Instance;

    private string _pubKey;
    private string _handshake = "";
    public string Handshake => _handshake;
    public string HandshakeURI => "tezos://?type=tzip10&data=" + _handshake;

    private bool _isConnected;
    public Action<bool> OnIsConnectedChanged;
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                OnIsConnectedChanged?.Invoke(value);
            }
        }
    }
    
    private void Awake()
    { 
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        // Create a BeaconMessageReceiver Game object to receive callback messages
        MessageReceiver = gameObject.AddComponent<WalletMessageReceiver>();
        var dataProviderConfig = new TzKTProviderConfig();
        API = new TezosDataAPI(dataProviderConfig);
        //Wallet = new WalletProvider();
        
        InitBeaconConnector();
        
        MessageReceiver.AccountConnected += Callback_OnAccountConnected;
        MessageReceiver.AccountConnectionFailed += Callback_OnAccountConnectionFailed;
        MessageReceiver.AccountDisconnected += Callback_OnAccountDisconnected;
        MessageReceiver.ContractCallCompleted += Callback_OnContractCallCompleted;
        MessageReceiver.ContractCallInjected += Callback_OnContractCallInjected;
        MessageReceiver.ContractCallFailed += Callback_OnContractCallFailed;
        MessageReceiver.PayloadSigned += Callback_OnPayloadSigned;
        MessageReceiver.HandshakeReceived += Callback_OnHandshakeReceived;
        MessageReceiver.PairingCompleted += Callback_OnPairingCompleted;
    }
    
    private void InitBeaconConnector()
    {
        // Assign the BeaconConnector depending on the platform.
#if UNITY_WEBGL && !UNITY_EDITOR
		BeaconConnector = new BeaconConnectorWebGl();
#else
        BeaconConnector = new BeaconConnectorDotNet();
        (BeaconConnector as BeaconConnectorDotNet)?.SetWalletMessageReceiver(MessageReceiver);
        Connect(WalletProviderType.beacon, withRedirectToWallet: false);
#endif
        
    }
    
    public void Connect(WalletProviderType walletProvider, bool withRedirectToWallet)
    {
        BeaconConnector.InitWalletProvider(
            network: TezosConfig.Instance.Network.ToString(),
            rpc: TezosConfig.Instance.RpcBaseUrl,
            walletProviderType: walletProvider);

        BeaconConnector.ConnectAccount();
#if UNITY_ANDROID || UNITY_IOS
            if (withRedirectToWallet)
                Application.OpenURL($"tezos://?type=tzip10&data={_handshake}");
#endif
    }
    
    public void Disconnect()
    {
        BeaconConnector.DisconnectAccount();
    }

    public string GetActiveAddress()
    {
        return BeaconConnector.GetActiveAccountAddress();
    }
    
    public IEnumerator GetTezosBalance(Action<ulong> callback, string address)
    {
        return API.GetTezosBalance(callback, address);
    }

    public void RequestSignPayload(SignPayloadType signingType, string payload)
    {
        BeaconConnector.RequestTezosSignPayload(signingType, payload);
    }

    public bool VerifySignedPayload(SignPayloadType signingType, string payload, string pubKey, string signature)
    {
        return NetezosExtensions.VerifySignature(pubKey, signingType, payload, signature);
    }
    
    public void CallContract(
        string contractAddress,
        string entryPoint,
        string input,
        ulong amount = 0)
    {
        BeaconConnector.RequestTezosOperation(
            destination: contractAddress,
            entryPoint: entryPoint,
            arg: input,
            amount: amount,
            networkName: TezosConfig.Instance.Network.ToString(),
            networkRPC: TezosConfig.Instance.RpcBaseUrl);
    }
    
    public struct TransactionResult
    {
        public bool success;
        public string transactionHash;
    }
    
    public IEnumerator TrackTransaction(string transactionHash, Action<TransactionResult> onTransactionCompleted, float timeoutInSeconds = 30, float secondsToWait = 1)
    {
        var success = false;
        var startTimestamp = Time.time;

        // keep making requests until time out or success
        while (!success && Time.time - startTimestamp < timeoutInSeconds)
        {
            Logger.LogDebug($"Checking tx status: {transactionHash}");
            yield return API.GetOperationStatus(result =>
            {
                if (result != null)
                    success = JsonSerializer.Deserialize<bool>(result);
            }, transactionHash);

            yield return new WaitForSecondsRealtime(secondsToWait);
        }

        TransactionResult result;
        result.success = success;
        result.transactionHash = transactionHash;
        onTransactionCompleted?.Invoke(result);
    }
    
    #region Callbacks
    
    private void Callback_OnAccountConnected(string account)
    {
        Debug.Log("AccountConnected: " + account);
        var json = JsonSerializer.Deserialize<JsonElement>(account);
        if (!json.TryGetProperty("accountInfo", out json)) return;

        _pubKey = json.GetProperty("publicKey").GetString();
        IsConnected = true;
    }
    
    private void Callback_OnAccountConnectionFailed(string result)
    {
        Debug.Log("AccountConnectionFailed: " + result);
    }
    
    private void Callback_OnAccountDisconnected(string result)
    {
        Debug.Log("AccountDisconnected: " + result);
        _pubKey = "";
        IsConnected = false;
    }
    
    private void Callback_OnContractCallCompleted(string result)
    {
        Debug.Log("ContractCallCompleted: " + result);
    }
    
    private void Callback_OnContractCallInjected(string transaction)
    {
        Debug.Log("ContractCallInjected: " + transaction);
        var json = JsonSerializer.Deserialize<JsonElement>(transaction);
        var transactionHash = json.GetProperty("transactionHash").GetString();

        CoroutineRunner.Instance.StartWrappedCoroutine(new CoroutineWrapper<object>(MessageReceiver.TrackTransaction(transactionHash)));
    }
    
    private void Callback_OnContractCallFailed(string result)
    {
        Debug.Log("ContractCallFailed: " + result);
    }
    
    private void Callback_OnPayloadSigned(string payload)
    {
        Debug.Log("PayloadSigned: " + payload);
        var json = JsonSerializer.Deserialize<JsonElement>(payload);
        var signature = json.GetProperty("signature").GetString();
    }
    
    private void Callback_OnHandshakeReceived(string handshake)
    {
        Debug.Log("HandshakeReceived: " + handshake);
        _handshake = handshake;
    }
    
    private void Callback_OnPairingCompleted(string result)
    {
        Debug.Log("PairingCompleted: " + result);
        BeaconConnector.RequestTezosPermission(networkName: TezosConfig.Instance.Network.ToString(), networkRPC: TezosConfig.Instance.RpcBaseUrl);
    }
    
    private void Callback_OnAccountReceived(string result)
    {
        Debug.Log("AccountReceived: " + result);
    }

    #endregion
}
