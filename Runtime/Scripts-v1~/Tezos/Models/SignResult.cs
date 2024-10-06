namespace TezosSDK.Tezos.Models
{

	/// <summary>
	///     Contains the result of a payload signing operation.
	/// </summary>
	public class SignResult
	{
		/// <summary>
		///     The message that was signed.
		/// </summary>
		public string Message;

		/// <summary>
		///     The signature resulting from the expression signing operation.
		/// </summary>
		public string Signature;
	}

}