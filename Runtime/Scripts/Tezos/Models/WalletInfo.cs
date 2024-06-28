using System;

namespace TezosSDK.Tezos.Models
{

	/// <summary>
	///     Contains information about a Tezos account, including the address and public key.
	/// </summary>
	public class WalletInfo
	{
		/// <summary>
		///     The Tezos wallet address.
		/// </summary>
		public string Address;

		/// <summary>
		///     The public key associated with the Tezos wallet.
		/// </summary>
		public string PublicKey;
	}

}