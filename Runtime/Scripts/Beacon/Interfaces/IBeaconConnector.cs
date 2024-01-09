using System;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Tezos;

namespace TezosSDK.Beacon
{

	/// <summary>
	///     Implement this interface to create a class that can connect to native code
	///     on a specific platform using the Beacon Sdk
	/// </summary>
	public interface IBeaconConnector
	{
		/// <summary>
		///     Raised when any wallet operation is requested
		/// </summary>
		event Action<BeaconMessageType> OperationRequested;

		/// <summary>
		///     Starts the connection between Beacon SDK and a wallet to connect to
		///     an account
		/// </summary>
		void ConnectWallet(WalletProviderType? walletProviderType);

		/// <summary>
		///     Checks if there is an active account paired.
		/// </summary>
		/// <returns>Returns only the account address as a string.</returns>
		public string GetWalletAddress();

		/// <summary>
		///     Allows waiting for permissions to be granted for pairing.
		/// </summary>
		public void RequestWalletConnection();

		/// <summary>
		///     Allows requesting a new operation such as sending tezos or initiating a smart contract.
		/// </summary>
		/// <param name="destination">The public ID to make a transaction to</param>
		/// <param name="entryPoint">The entry point if one needs to be specified</param>
		/// <param name="arg">The arguments if any are needed</param>
		/// <param name="amount">Ammount to be sent, if Tez, it's calculated in MuTez</param>
		public void RequestOperation(
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
		public void RequestSignPayload(SignPayloadType signingType, string payload);

		/// <summary>
		///     Removes the connection to the current active account.
		/// </summary>
		void DisconnectWallet();
	}

}