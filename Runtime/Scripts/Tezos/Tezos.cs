using System;
using System.Collections;
using System.Collections.Generic;
using TezosSDK.Beacon;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos
{

	/// <summary>
	///     Tezos API and Wallet features
	///     Exposes the main functions of the Tezos in Unity
	/// </summary>
	public class Tezos : ITezos
	{
		public IWalletConnection WalletConnection { get; }
		public IWalletAccount WalletAccount { get; }
		public IWalletTransaction WalletTransaction { get; }
		public IWalletContract WalletContract { get; }
		public IWalletEventProvider WalletEventProvider { get; }
		
		public Tezos(TezosConfigSO config, WalletProvider walletProvider)
		{
			API = new TezosAPI(config);

			WalletConnection = walletProvider;
			WalletAccount = walletProvider;
			WalletTransaction = walletProvider;
			WalletContract = walletProvider;
			WalletEventProvider = walletProvider;

			// Subscribe to wallet events
			walletProvider.EventManager.WalletConnected += OnWalletConnected;
		}

		public ITezosAPI API { get; }
		public IFa2 TokenContract { get; set; }

		public IEnumerator GetCurrentWalletBalance(Action<Result<ulong>> callback)
		{
			var address = WalletAccount.GetWalletAddress();
			yield return API.GetTezosBalance(callback, address);
		}

		public IEnumerator GetOriginatedContracts(Action<Result<IEnumerable<TokenContract>>> callback)
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
				Logger.LogInfo("Found deployed contract address in player prefs: " + address);
			}

			if (TokenContract != null)
			{
				return;
			}

			TokenContract = !string.IsNullOrEmpty(address)
				? new TokenContract(address, WalletAccount, WalletTransaction, WalletContract, WalletEventProvider, API)
				: new TokenContract(WalletAccount, WalletTransaction, WalletContract, WalletEventProvider, API);
		}
	}

}