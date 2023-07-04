using System;
using System.Collections;
using TezosSDK.Beacon;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models;
using TezosSDK.Tezos.Wallet;
using UnityEngine;

namespace TezosSDK.Tezos
{
    /// <summary>
    /// Tezos API and Wallet features
    /// Exposes the main functions of the Tezos in Unity
    /// </summary>
    public class Tezos : ITezos
    {
        public WalletMessageReceiver MessageReceiver { get; }
        public ITezosAPI API { get; }
        public IWalletProvider Wallet { get; }
        public TokenContract TokenContract { get; }

        public Tezos()
        {
            var dataProviderConfig = new TzKTProviderConfig();
            API = new TezosAPI(dataProviderConfig);
            Wallet = new WalletProvider();

            MessageReceiver = Wallet.MessageReceiver;

            TokenContract = PlayerPrefs.HasKey("CurrentContract")
                ? new TokenContract(PlayerPrefs.GetString("CurrentContract"))
                : new TokenContract();
        }
        
        public IEnumerator GetCurrentWalletBalance(Action<ulong> callback)
        {
            var address = Wallet.GetActiveAddress();
            return API.GetTezosBalance(callback, address);
        }
    }
}