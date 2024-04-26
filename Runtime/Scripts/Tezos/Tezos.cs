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
		public Tezos(TezosConfigSO config, IWalletEventManager eventManager, IBeaconConnector beaconConnector)
		{
			API = new TezosAPI(config);

			Wallet = new WalletProvider(eventManager, beaconConnector);
			Wallet.EventManager.WalletConnected += OnWalletConnected;
		}

		public ITezosAPI API { get; }
		public IWalletProvider Wallet { get; }
		public IFa2 TokenContract { get; set; }

		public IEnumerator GetCurrentWalletBalance(Action<Result<ulong>> callback)
		{
			var address = Wallet.GetWalletAddress();
			yield return API.GetTezosBalance(callback, address);
		}

		public IEnumerator GetOriginatedContracts(Action<Result<IEnumerable<TokenContract>>> callback)
		{
			var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;

			return API.GetOriginatedContractsForOwner(callback, Wallet.GetWalletAddress(), codeHash, 1000,
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
			
			if (TokenContract != null) return;

			TokenContract = !string.IsNullOrEmpty(address)
				// if there is a contract address in the player prefs, use it
				? new TokenContract(address)
				// otherwise, create a new contract
				: new TokenContract();
		}
	}

}