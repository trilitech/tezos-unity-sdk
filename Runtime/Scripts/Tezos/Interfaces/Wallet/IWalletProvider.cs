using Beacon.Sdk.Beacon.Sign;

namespace TezosSDK.Tezos.Wallet
{

	public interface
		IWalletProvider // TODO: Some of these need to be moved to IBeaconConnector and/or to another interface
	{
		/// <summary>
		///     Exposes a MonoBehaviour class that exposes wallet events
		/// </summary>
		IWalletEventManager EventManager { get; }

		bool IsConnected { get; }

		HandshakeData HandshakeData { get; }

		/// <summary>
		///     Makes a call to connect with a wallet
		/// </summary>
		void Connect(WalletProviderType walletProvider);

		/// <summary>
		///     Unpair with wallet and disconnect
		/// </summary>
		void Disconnect();

		/// <summary>
		///     Returns the address of the current active wallet
		/// </summary>
		/// <returns></returns>
		string GetWalletAddress();

		/// <summary>
		///     Sends a request to the sign a payload string
		/// </summary>
		/// <param name="signingType">type of payload: raw, operation or micheline</param>
		/// <param name="payload">payload string that is going to be signed</param>
		void RequestSignPayload(SignPayloadType signingType, string payload);

		/// <summary>
		///     Verify a signed payload to check if it is valid
		/// </summary>
		/// <param name="signingType">type of payload: raw, operation or micheline</param>
		/// <param name="payload">payload string that is going to be signed</param>
		bool VerifySignedPayload(SignPayloadType signingType, string payload);

		/// <summary>
		///     Performs an operation in the contract
		/// </summary>
		/// <param name="contractAddress">destination address of the smart contract</param>
		/// <param name="entryPoint">entry point used in the smart contract</param>
		/// <param name="input">parameters called on the entry point</param>
		/// <param name="amount">amount of Tez sent into the contract</param>
		void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0);

		/// <summary>
		///     Originate new contract.
		/// </summary>
		/// <param name="script">Code of contract</param>
		/// <param name="delegateAddress">Delegator address</param>
		void OriginateContract(string script, string delegateAddress = null);
	}

}