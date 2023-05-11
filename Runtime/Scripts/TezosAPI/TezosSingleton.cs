using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;
using TezosAPI;
using TezosAPI.Models;
using TezosAPI.Models.Tokens;


public class TezosSingleton : SingletonMonoBehaviour<TezosSingleton>, ITezosAPI
{
    private static Tezos _tezos;

    public BeaconMessageReceiver MessageReceiver => _tezos.MessageReceiver;

    public string NetworkRPC => _tezos.NetworkRPC;

    protected override void Awake()
    {
        base.Awake();

        _tezos = new Tezos();
    }

    public void ConnectWallet()
    {
        _tezos.ConnectWallet();
    }

    public void DisconnectWallet()
    {
        _tezos.DisconnectWallet();
    }

    public string GetActiveWalletAddress()
    {
        return _tezos.GetActiveWalletAddress();
    }

    public IEnumerator ReadBalance(Action<ulong> callback)
    {
        return _tezos.ReadBalance(callback);
    }

    public IEnumerator ReadView(
        string contractAddress,
        string entryPoint,
        object input,
        Action<JsonElement> callback
    )
    {
        return _tezos.ReadView(contractAddress, entryPoint, input, callback);
    }

    public void CallContract(
        string contractAddress,
        string entryPoint,
        string input,
        ulong amount
    )
    {
        _tezos.CallContract(contractAddress, entryPoint, input, amount);
    }

    public void RequestPermission()
    {
        _tezos.RequestPermission();
    }

    public void RequestSignPayload(SignPayloadType signingType, string payload)
    {
        _tezos.RequestSignPayload(signingType, payload);
    }

    public bool VerifySignedPayload(SignPayloadType signingType, string payload)
    {
        return _tezos.VerifySignedPayload(signingType, payload);
    }

    public IEnumerator GetTokensForOwner(
        Action<IEnumerable<TokenBalance>> cb,
        string owner,
        bool withMetadata,
        long maxItems,
        TokensForOwnerOrder orderBy
    ) => _tezos.GetTokensForOwner(cb, owner, withMetadata, maxItems, orderBy);
}