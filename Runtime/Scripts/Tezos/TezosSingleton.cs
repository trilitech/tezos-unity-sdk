using System;
using System.Collections;
using Beacon.Sdk.Beacon.Permission;
using Scripts.Helpers;
using Scripts.Tezos.API;
using Scripts.Tezos.Wallet;

namespace Scripts.Tezos
{
    public class TezosSingleton : SingletonMonoBehaviour<TezosSingleton>, ITezos
    {
        private static ITezos _tezos;
        public ITezosAPI API => _tezos.API;
        public IWalletProvider Wallet => _tezos.Wallet;

        protected override void Awake()
        {
            base.Awake();

            Logger.CurrentLogLevel = Logger.LogLevel.Debug;
            TezosConfig.Instance.Network = NetworkType.ghostnet;
            _tezos = new Tezos();
        }

        public IEnumerator GetCurrentWalletBalance(Action<ulong> callback) => _tezos.GetCurrentWalletBalance(callback);
    }
}