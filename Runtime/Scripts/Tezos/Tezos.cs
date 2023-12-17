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
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos
{
    /// <summary>
    /// Tezos API and Wallet features
    /// Exposes the main functions of the Tezos in Unity
    /// </summary>
    public class Tezos : ITezos
    {
        public ITezosAPI API { get; }
        public IWalletProvider Wallet { get; }
        public IFA2 TokenContract { get; set; }

        public Tezos(DAppMetadata dAppMetadata, bool redirectToWallet)
        {
            var dataProviderConfig = new TzKTProviderConfig();
            API = new TezosAPI(dataProviderConfig);
            
            Wallet = new WalletProvider(dAppMetadata, withRedirectToWallet: redirectToWallet);
            Wallet.EventManager.AccountConnected += OnAccountConnected;
        }

        private void OnAccountConnected(AccountInfo accountInfo)
        {
            var hasKey = PlayerPrefs.HasKey("CurrentContract:" + accountInfo.Address);

            var address = hasKey ? PlayerPrefs.GetString("CurrentContract:" + accountInfo.Address) : string.Empty;

            if (hasKey)
            {
                Logger.LogDebug("Found contract address in player prefs: " + address);
            }

            TokenContract = !string.IsNullOrEmpty(address)
                // if there is a contract address in the player prefs, use it
                ? new TokenContract(address)
                // otherwise, create a new contract
                : new TokenContract();
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