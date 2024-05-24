using System;
using System.Collections;
using System.Collections.Generic;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.Filters;
using TezosSDK.Tezos.Interfaces;
using TezosSDK.Tezos.Interfaces.API;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.ScriptableObjects;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Interfaces;
using UnityEngine;

namespace TezosSDK.Tezos.Core
{

	/// <summary>
	///     Tezos API and Wallet features
	///     Exposes the main functions of the Tezos in Unity
	/// </summary>
	public class Tezos : ITezos
	{
		
		
		public Tezos(TezosConfigSO config, WalletProvider walletProvider)
			: this(config, walletProvider, walletProvider, walletProvider, walletProvider, walletProvider)
		{
		}

		public Tezos(TezosConfigSO config, IWalletConnection walletConnection, IWalletAccount walletAccount, IWalletTransaction walletTransaction, IWalletContract walletContract, IWalletEventProvider eventProvider)
		{
			API = new TezosAPI(config);

			WalletConnection = walletConnection;
			WalletAccount = walletAccount;
			WalletTransaction = walletTransaction;
			WalletContract = walletContract;
			WalletEventProvider = eventProvider;
			// Subscribe to wallet events
			WalletEventProvider.EventManager.WalletConnected += OnWalletConnected;
		}

		public IWalletConnection WalletConnection { get; }
		public IWalletAccount WalletAccount { get; }
		public IWalletTransaction WalletTransaction { get; }
		public IWalletContract WalletContract { get; }
		public IWalletEventProvider WalletEventProvider { get; }
		public ITezosAPI API { get; }
		public IFa2 TokenContract { get; set; }

		public IEnumerator GetCurrentWalletBalance(Action<HttpResult<ulong>> callback)
		{
			var address = WalletAccount.GetWalletAddress();
			yield return API.GetTezosBalance(callback, address);
		}

		public IEnumerator GetOriginatedContracts(Action<HttpResult<IEnumerable<TokenContract>>> callback)
		{
			var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;

			return API.GetOriginatedContractsForOwner(callback, WalletAccount.GetWalletAddress(), codeHash, 1000,
				new OriginatedContractsForOwnerOrder.ByLastActivityTimeDesc(0));
		}

		private void OnWalletConnected(WalletInfo walletInfo)
		{
			var hasKey = PlayerPrefs.HasKey("CurrentContract:" + walletInfo.Address);

			var address = hasKey ? PlayerPrefs.GetString("CurrentContract:" + walletInfo.Address) : string.Empty;

			if (hasKey)
			{
				TezosLogger.LogInfo("Found deployed contract address in player prefs: " + address);
			}

			if (TokenContract != null)
			{
				return;
			}

			TokenContract = !string.IsNullOrEmpty(address)
				? new TokenContract(address, WalletAccount, WalletTransaction, WalletContract, API)
				: new TokenContract(WalletAccount, WalletTransaction, WalletContract, API);
		}
	}

}