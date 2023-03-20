namespace BeaconSDK
{
	/// <summary>
	/// Implement this interface to create a class that can connect to native code
	/// on a specific platform using the Beacon Sdk
	/// </summary>
	public interface IBeaconConnector
	{
		/// <summary>
		/// Specifies to which network the Beacon Client should connect
		/// (Take in consideration that not all wallets support all available networks)
		/// </summary>
		/// <param name="network">Name of the network to connect</param>
		/// <param name="rpc">Uri of an specific RPC. (It can be left empty to use
		/// the default RPC</param>
		void SetNetwork(string network, string rpc);

		/// <summary>
		/// Starts the connection between Beacon SDK and a wallet to connect to
		/// an account
		/// </summary>
		void ConnectAccount();

		/// <summary>
		/// Checks if there is an active account paired.
		/// </summary>
		/// <returns>Returns only the account address as a string.</returns>
		public string GetActiveAccountAddress();

		/// <summary>
		/// Requests a handshake to be used in QR and Deeplink Pairing (same as the previously removed Pair)
		/// </summary>
		void RequestHandshake();

		/// <summary>
		/// Allows waiting for permissions to be granted for pairing.
		/// </summary>
		/// <param name="networkName">The name of the desired network.</param>
		/// <param name="networkRPC">The RPC to the desired network</param>
		public void RequestTezosPermission(string networkName = "", string networkRPC = "");

		/// <summary>
		/// Allows requesting a new operation such as sending tezos or initiating a smart contract.
		/// </summary>
		/// <param name="destination">The public ID to make a transaction to</param>
		/// <param name="entryPoint">The entry point if one needs to be specified</param>
		/// <param name="arg">The arguments if any are needed</param>
		/// <param name="amount">Ammount to be sent, if Tez, it's calculated in MuTez</param>
		/// <param name="networkName">The name of the desired network.</param>
		/// <param name="networkRPC">The RPC to the desired network</param>
		public void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null,
			ulong amount = 0, string networkName = "", string networkRPC = "");

		/// <summary>
		/// To make a request to sign a payload
		/// </summary>
		/// <param name="signingType">An integer to select a SigningType: 0=Raw 1=Operation 2=Micheline allelse=Micheline</param>
		/// <param name="payload"></param>
		public void RequestTezosSignPayload(int signingType, string payload);

		/// <summary>
		/// To use when broadcasts are required
		/// </summary>
		/// <param name="signedTransaction">The signed transaction to be broadcasted</param>
		/// <param name="networkName">The name of the desired network.</param>
		/// <param name="networkRPC">The RPC to the desired network</param>
		public void RequestTezosBroadcast(string signedTransaction, string networkName = "", string networkRPC = "");

		/// <summary>
		/// Removes the connection to the current active account.
		/// </summary>
		void DisconnectAccount();
	}
}
