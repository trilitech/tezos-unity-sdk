using System;
using System.Collections;
using Scripts.Tezos.API;
using Scripts.Tezos.Wallet;

namespace Scripts.Tezos
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
        /// Fetch current wallet Tezos balance in micro tez
        /// </summary>
        /// <param name="callback">callback action that runs with the ulong balance is fetched</param>
        /// <returns></returns>
        IEnumerator GetCurrentWalletBalance(Action<ulong> callback);
    }
}