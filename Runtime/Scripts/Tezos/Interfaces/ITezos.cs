using System;
using System.Collections;
using System.Collections.Generic;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.Wallet;

namespace TezosSDK.Tezos
{

	public interface ITezos
	{
		/// <summary>
		///     Tezos chain data source
		/// </summary>
		ITezosAPI API { get; }

		/// <summary>
		///     Wallet features
		/// </summary>
		IWalletProvider Wallet { get; }

		/// <summary>
		///     Currently used FA2 Contract.
		/// </summary>
		IFa2 TokenContract { get; set; }

		/// <summary>
		///     Current wallet tz balance.
		/// </summary>
		public IEnumerator GetCurrentWalletBalance(Action<ulong> callback);

		/// <summary>
		///     Get all originated contracts by the account.
		/// </summary>
		public IEnumerator GetOriginatedContracts(Action<IEnumerable<TokenContract>> callback);
	}

}