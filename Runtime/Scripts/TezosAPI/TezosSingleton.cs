using System;
using System.Collections;
using System.Text.Json;
using BeaconSDK;
using TezosAPI;

public class TezosSingleton : SingletonMonoBehaviour<TezosSingleton>, ITezosAPI
{
    private static Tezos _tezos;

    BeaconMessageReceiver ITezosAPI.MessageReceiver => _tezos.MessageReceiver;

    public string NetworkRPC => _tezos.NetworkRPC;

    protected override void Awake()
    {
        base.Awake();

        _tezos = new Tezos();
    }

    void ITezosAPI.ConnectWallet()
    {
        _tezos.ConnectWallet();
    }

    void ITezosAPI.DisconnectWallet()
    {
        _tezos.DisconnectWallet();
    }

    string ITezosAPI.GetActiveWalletAddress()
    {
        return _tezos.GetActiveWalletAddress();
    }

    IEnumerator ITezosAPI.ReadBalance(Action<ulong> callback)
    {
        return _tezos.ReadBalance(callback);
    }

    IEnumerator ITezosAPI.ReadView(
        string contractAddress,
        string entryPoint,
        object input,
        Action<JsonElement> callback
    )
    {
        return _tezos.ReadView(contractAddress, entryPoint, input, callback);
    }

    void ITezosAPI.CallContract(
        string contractAddress,
        string entryPoint,
        string input,
        ulong amount
    )
    {
        _tezos.CallContract(contractAddress, entryPoint, input, amount);
    }

    void ITezosAPI.RequestPermission()
    {
        _tezos.RequestPermission();
    }

    void ITezosAPI.RequestSignPayload(int signingType, string payload)
    {
        _tezos.RequestSignPayload(signingType, payload);
    }

    public bool VerifySignedPayload(string payload)
    {
        return _tezos.VerifySignedPayload(payload);
    }
}