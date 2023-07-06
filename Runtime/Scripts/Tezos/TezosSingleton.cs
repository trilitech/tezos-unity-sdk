using System;
using System.Collections;
using System.Collections.Generic;
using Beacon.Sdk.Beacon.Permission;
using TezosSDK.DesignPattern.Singleton;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Abstract;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;


namespace TezosSDK.Tezos
{
    public class TezosSingleton : SingletonMonoBehaviour<TezosSingleton>, ITezos
    {
        private static Tezos _tezos;
        public ITezosAPI API => _tezos?.API;
        public IWalletProvider Wallet => _tezos?.Wallet;
        public IFA2 TokenContract => _tezos?.TokenContract;

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

        public IEnumerator GetOriginatedContracts(Action<IEnumerable<TokenContract>> callback)
        {
            var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash")
                .text;
            
            return _tezos.API.GetOriginatedContractsForOwner(
                callback: callback,
                creator: Wallet.GetActiveAddress(),
                codeHash: codeHash,
                maxItems: 1000,
                orderBy: new OriginatedContractsForOwnerOrder.ByLastActivityTimeDesc(0));
        }
    }
}