namespace TezosSDK.Tezos.Models
{

	/// <summary>
	///     Contains information related to the successful completion of a pairing between the Tezos wallet and a DApp.
	/// </summary>
	public class PairingDoneData
	{
		/// <summary>
		///     The public key of the paired DApp, which is used to identify the DApp on the Tezos blockchain.
		/// </summary>
		public string DAppPublicKey; // Public key of the paired DApp

		/// <summary>
		///     The timestamp indicating the exact time when the pairing was completed. This can be used for logging or
		///     verification purposes.
		/// </summary>
		public string Timestamp; // Timestamp to be possibly used for logging actions
	}

}