namespace TezosSDK.Tezos.Models
{

	/// <summary>
	///     Represents data required for the wallet to complete the pairing process with a decentralized application (DApp).
	/// </summary>
	public class PairingRequestData
	{
		/// <summary>
		///     The data needed for the user to complete the pairing, which may be a QR code or deep link.
		/// </summary>
		public string Data;
	}

}