using TezosSDK.WalletServices.Connectors;

namespace TezosSDK.Tezos.Models
{

	/// <summary>
	///     Contains information about a Tezos account, including the address and public key.
	/// </summary>
	public class WalletInfo
	{
		public ConnectorType ConnectorType;
		
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