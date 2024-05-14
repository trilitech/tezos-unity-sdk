using System;
using System.Collections;
using System.Collections.Generic;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Tezos.API;
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
		/// Connection to the wallet
		/// </summary>
		IWalletConnection WalletConnection { get; }

		/// <summary>
		/// Account information for the wallet
		/// </summary>
		IWalletAccount WalletAccount { get; }

		/// <summary>
		/// Transaction-related functionalities for the wallet
		/// </summary>
		IWalletTransaction WalletTransaction { get; }

		/// <summary>
		/// Contract-related functionalities for the wallet
		/// </summary>
		IWalletContract WalletContract { get; }
		
		/// <summary>
		/// Event management for wallet-related events
		/// </summary>
		IWalletEventProvider WalletEventProvider { get; }

		/// <summary>
		/// Currently used FA2 Contract.
		/// </summary>
		IFa2 TokenContract { get; set; }

		/// <summary>
		/// Current wallet tz balance.
		/// </summary>
		IEnumerator GetCurrentWalletBalance(Action<Result<ulong>> callback);

		/// <summary>
		/// Get all originated contracts by the account.
		/// </summary>
		IEnumerator GetOriginatedContracts(Action<Result<IEnumerable<TokenContract>>> callback);
	}

}