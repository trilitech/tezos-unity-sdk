using System;
using System.Collections;
using Scripts.Tezos.API;
using Scripts.Tezos.Wallet;


namespace Scripts.Tezos
{
    /// <summary>
    /// Tezos API and Wallet features
    /// Exposes the main functions of the Tezos in Unity
    /// </summary>
    public class Tezos : ITezos
    {
        public ITezosAPI API { get; }
        public IWalletProvider Wallet { get; }

        public Tezos()
        {
            API = new TezosAPI();
            Wallet = new BeaconWalletProvider();
        }

        public IEnumerator GetCurrentWalletBalance(Action<ulong> callback)
        {
            var address = Wallet.GetActiveAddress();
            return API.GetTezosBalance(callback, address);
        }
    }
}