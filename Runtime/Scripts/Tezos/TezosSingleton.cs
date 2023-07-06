using System;
using System.Collections;
using Beacon.Sdk.Beacon.Permission;
using TezosSDK.DesignPattern.Singleton;
using TezosSDK.Helpers;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models;
using TezosSDK.Tezos.Wallet;


namespace TezosSDK.Tezos
{
    public class TezosSingleton : SingletonMonoBehaviour<TezosSingleton>, ITezos
    {
        private static Tezos _tezos;
        public ITezosAPI API => _tezos.API;
        public IWalletProvider Wallet => _tezos.Wallet;
        public TokenContract TokenContract => _tezos.TokenContract;

        protected override void Awake()
        {
            base.Awake();

            Logger.CurrentLogLevel = Logger.LogLevel.Debug;
            TezosConfig.Instance.Network = NetworkType.ghostnet;
            _tezos = new Tezos();
        }
        
        void OnApplicationQuit()
        {
            if (Wallet is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public IEnumerator GetCurrentWalletBalance(Action<ulong> callback)
        {
            return _tezos.GetCurrentWalletBalance(callback);
        }
    }
}