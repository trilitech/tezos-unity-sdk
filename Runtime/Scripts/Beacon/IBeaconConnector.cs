#region

using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;

#endregion

namespace TezosSDK.Beacon
{

	/// <summary>
	///     Implement this interface to create a class that can connect to native code
	///     on a specific platform using the Beacon Sdk
	/// </summary>
	public interface IBeaconConnector
	{
		/// <summary>
		///     Initially configure wallet before using.
		/// </summary>
		/// <param name="network">Name of the network to connect</param>
		/// <param name="rpc">Uri of an specific RPC.</param>
		/// <param name="walletProviderType">Type of wallet, e.g. "beacon" or "kukai"</param>
		/// <param name="metadata">Metadata of SDK consumer DApp</param>
		void InitWalletProvider(
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata metadata);

		/// <summary>
		///     Callback that needed in WebGL to determine that UI is rendered
		/// </summary>
		void OnReady();

		/// <summary>
		///     Starts the connection between Beacon SDK and a wallet to connect to
		///     an account
		/// </summary>
		void ConnectWallet();

		/// <summary>
		///     Checks if there is an active account paired.
		/// </summary>
		/// <returns>Returns only the account address as a string.</returns>
		public string GetActiveWalletAddress();

		/// <summary>
		///     Allows waiting for permissions to be granted for pairing.
		/// </summary>
		public void RequestTezosPermission();

		/// <summary>
		///     Allows requesting a new operation such as sending tezos or initiating a smart contract.
		/// </summary>
		/// <param name="destination">The public ID to make a transaction to</param>
		/// <param name="entryPoint">The entry point if one needs to be specified</param>
		/// <param name="arg">The arguments if any are needed</param>
		/// <param name="amount">Ammount to be sent, if Tez, it's calculated in MuTez</param>
		public void RequestTezosOperation(
			string destination,
			string entryPoint = "default",
			string arg = null,
			ulong amount = 0);

		/// <summary>
		///     Originate new contract
		/// </summary>
		/// <param name="script">Contract code.</param>
		/// <param name="delegateAddress">Delegator address</param>
		public void RequestContractOrigination(string script, string delegateAddress = null);

		/// <summary>
		///     To make a request to sign a payload
		/// </summary>
		/// <param name="signingType">An integer to select a SigningType: 0=Raw 1=Operation 2=Micheline allelse=Micheline</param>
		/// <param name="payload"></param>
		public void RequestTezosSignPayload(SignPayloadType signingType, string payload);

		/// <summary>
		///     Removes the connection to the current active account.
		/// </summary>
		void DisconnectWallet();
	}

}