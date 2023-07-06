using System;
using System.Collections;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models;
using TezosSDK.Tezos.API.Models.Abstract;
using TezosSDK.Tezos.Wallet;

namespace TezosSDK.Tezos
{
    public interface ITezos
    {
        /// <summary>
        /// Tezos chain data source
        /// </summary>
        ITezosAPI API { get; }

        /// <summary>
        /// Wallet features
        /// </summary>
        IWalletProvider Wallet { get; }

        /// <summary>
        /// Currently used FA2 Contract.
        /// </summary>
        IFA2 TokenContract { get; }

        /// <summary>
        /// Current wallet tz balance.
        /// </summary>
        public IEnumerator GetCurrentWalletBalance(Action<ulong> callback);
    }
}