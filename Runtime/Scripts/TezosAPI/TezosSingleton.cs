using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;
using Helpers;
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
        Logger.CurrentLogLevel = Logger.LogLevel.Info;
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
        Action<IEnumerable<TokenBalance>> callback,
        string owner,
        bool withMetadata,
        long maxItems,
        TokensForOwnerOrder orderBy) => _tezos.GetTokensForOwner(callback, owner, withMetadata, maxItems, orderBy);

    public IEnumerator GetOwnersForToken(
        Action<IEnumerable<TokenBalance>> callback,
        string contractAddress,
        uint tokenId,
        long maxItems,
        OwnersForTokenOrder orderBy) => _tezos.GetOwnersForToken(callback, contractAddress, tokenId, maxItems, orderBy);

    public IEnumerator GetOwnersForContract(
        Action<IEnumerable<TokenBalance>> callback,
        string contractAddress,
        long maxItems,
        OwnersForContractOrder orderBy) => _tezos.GetOwnersForContract(callback, contractAddress, maxItems, orderBy);

    public IEnumerator IsHolderOfContract(
        Action<bool> callback,
        string wallet,
        string contractAddress) => _tezos.IsHolderOfContract(callback, wallet, contractAddress);

    public IEnumerator IsHolderOfToken(Action<bool> callback,
        string wallet,
        string contractAddress,
        uint tokenId) => _tezos.IsHolderOfToken(callback, wallet, contractAddress, tokenId);

    public IEnumerator GetTokenMetadata(
        Action<JsonElement> callback,
        string contractAddress,
        uint tokenId) => _tezos.GetTokenMetadata(callback, contractAddress, tokenId);

    public IEnumerator GetContractMetadata(
        Action<JsonElement> callback,
        string contractAddress) => _tezos.GetContractMetadata(callback, contractAddress);

    public IEnumerator GetTokensForContract(
        Action<IEnumerable<Token>> callback,
        string contractAddress,
        bool withMetadata,
        long maxItems,
        TokensForContractOrder orderBy) =>
        _tezos.GetTokensForContract(callback, contractAddress, withMetadata, maxItems, orderBy);
}