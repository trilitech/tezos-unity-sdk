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

    void ITezosAPI.RequestSignPayload(SignPayloadType signingType, string payload)
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
        TokensForOwnerOrder orderBy) => _tezos.GetTokensForOwner(cb, owner, withMetadata, maxItems, orderBy);

    public IEnumerator GetOwnersForToken(
        Action<IEnumerable<TokenBalance>> cb,
        string contractAddress,
        uint tokenId,
        long maxItems,
        OwnersForTokenOrder orderBy) => _tezos.GetOwnersForToken(cb, contractAddress, tokenId, maxItems, orderBy);

    public IEnumerator GetOwnersForContract(
        Action<IEnumerable<TokenBalance>> cb,
        string contractAddress,
        long maxItems,
        OwnersForContractOrder orderBy) => _tezos.GetOwnersForContract(cb, contractAddress, maxItems, orderBy);

    public IEnumerator IsHolderOfContract(
        Action<bool> cb,
        string wallet,
        string contractAddress) => _tezos.IsHolderOfContract(cb, wallet, contractAddress);

    public IEnumerator IsHolderOfToken(
        Action<bool> cb,
        string wallet,
        string contractAddress,
        string tokenId) => _tezos.IsHolderOfToken(cb, wallet, contractAddress, tokenId);

    public IEnumerator GetTokenMetadata(
        Action<JsonElement> cb,
        string contractAddress,
        uint tokenId) => _tezos.GetTokenMetadata(cb, contractAddress, tokenId);

    public IEnumerator GetContractMetadata(
        Action<JsonElement> cb,
        string contractAddress) => _tezos.GetContractMetadata(cb, contractAddress);

    public IEnumerator GetTokensForContract(
        Action<IEnumerable<Token>> cb,
        string contractAddress,
        bool withMetadata,
        long maxItems,
        TokensForContractOrder orderBy) =>
        _tezos.GetTokensForContract(cb, contractAddress, withMetadata, maxItems, orderBy);
}