using System;
using System.Collections;
using System.Collections.Generic;
using TezosSDK.Beacon;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Abstract;
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
        public WalletEventManager EventManager { get; }
        public ITezosAPI API { get; }
        public IWalletProvider Wallet { get; }
        public IFA2 TokenContract { get; set; }

        public Tezos(DAppMetadata dAppMetadata)
        {
            var dataProviderConfig = new TzKTProviderConfig();
            API = new TezosAPI(dataProviderConfig);
            
            Wallet = new WalletProvider(dAppMetadata);

            EventManager = Wallet.EventManager;
            EventManager.AccountConnected += _ =>
            {
                TokenContract = PlayerPrefs.HasKey("CurrentContract:" + Wallet.GetActiveAddress())
                    ? new TokenContract(PlayerPrefs.GetString("CurrentContract:" + Wallet.GetActiveAddress()))
                    : new TokenContract();
            };
        }

        public IEnumerator GetCurrentWalletBalance(Action<ulong> callback)
        {
            var address = Wallet.GetActiveAddress();
            return API.GetTezosBalance(callback, address);
        }

        public IEnumerator GetOriginatedContracts(Action<IEnumerable<TokenContract>> callback)
        {
            var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash")
                .text;

            return API.GetOriginatedContractsForOwner(
                callback: callback,
                creator: Wallet.GetActiveAddress(),
                codeHash: codeHash,
                maxItems: 1000,
                orderBy: new OriginatedContractsForOwnerOrder.ByLastActivityTimeDesc(0));
        }
    }
}