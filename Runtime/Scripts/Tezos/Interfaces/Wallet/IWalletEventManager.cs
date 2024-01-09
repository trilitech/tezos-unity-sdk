using System;

namespace TezosSDK.Tezos.Wallet
{

	public interface IWalletEventManager
	{
		/// <summary>
		///     Processes the incoming event data and dispatches it to the corresponding event
		///     based on the <see cref="UnifiedEvent.EventType" />.
		/// </summary>
		/// <param name="unifiedEvent">
		///     The <see cref="UnifiedEvent" /> to be handled, which contains
		///     the event type and data (if any).
		/// </param>
		void HandleEvent(UnifiedEvent unifiedEvent);

		/// <summary>
		///     Runs when a call to a smart contract is confirmed on the blockchain. Provides the result of the call.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="OperationResult" /> object with the transaction hash and success status.
		///     This event is fired when a blockchain operation completes and has been injected into the blockchain.
		///     The result includes the transaction hash and an indication of whether it was successful.
		/// </remarks>
		event Action<OperationResult> ContractCallCompleted;

		/// <summary>
		///     Runs when a call to a smart contract fails. Provides error details.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="ErrorInfo" /> object containing the error message of the failed contract call.
		///     It is triggered when a contract call attempted by the wallet encounters an error.
		/// </remarks>
		event Action<ErrorInfo> ContractCallFailed;

		/// <summary>
		///     Runs when a call to a smart contract is sent to Tezos but before it has been included in a block and confirmed.
		///     Provides the hash of the transaction.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="OperationResult" /> object containing the transaction hash and success status after the
		///     contract call is injected.
		///     It is triggered after an operation request (like a contract call) is sent successfully to the blockchain network.
		/// </remarks>
		event Action<OperationResult> ContractCallInjected;

		/// <summary>
		///     Runs when a handshake with a user's wallet application is received. Provides the handshake details.
		/// </summary>
		/// <remarks>
		///     Provides a <see cref="HandshakeData" /> object with the pairing information required for user completion.
		///     This event is triggered as part of the pairing process. The handshake data may include a QR code or other
		///     information necessary to complete pairing with a DApp.
		/// </remarks>
		event Action<HandshakeData> HandshakeReceived;

		/// <summary>
		///     Runs when the user's wallet is connected but before the user has approved the connection in the wallet app.
		///     Provides details of the pairing completion.
		///     Note: This event is not supported in WebGL builds.
		/// </summary>
		/// <remarks>
		///     Provides a <see cref="PairingDoneData" /> object with details about the pairing, such as the DApp's public key and
		///     the timestamp of pairing completion.
		///     Triggered when the pairing between the Tezos wallet and a DApp is completed. Reveals public key and a timestamp
		///     indicating when the pairing was finalized.
		/// </remarks>
		event Action<PairingDoneData> PairingCompleted;

		/// <summary>
		///     Runs when the user signs a payload. Provides the sign result.
		/// </summary>
		/// <remarks>
		///     Provides a <see cref="SignResult" /> object containing the signature value.
		///     The event is triggered in response to a successful payload signing request.
		/// </remarks>
		event Action<SignResult> PayloadSigned;

		/// <summary>
		///     Runs when an account connects successfully. Provides the account information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="WalletInfo" /> object containing the address and public key of the connected account.
		///     It is triggered in response to a successful connection action from the wallet.
		/// </remarks>
		event Action<WalletInfo> WalletConnected;

		/// <summary>
		///     Runs when a connection to an account fails. Provides error information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="ErrorInfo" /> object containing the error message of the failed connection attempt.
		///     It is triggered when a connection attempt to an account encounters an error.
		/// </remarks>
		event Action<ErrorInfo> WalletConnectionFailed;

		/// <summary>
		///     Runs when an account disconnects successfully. Provides the account information.
		/// </summary>
		/// <remarks>
		///     Provides an <see cref="WalletInfo" /> object containing the address and public key of the disconnected account.
		///     It is triggered in response to a successful disconnection action from the wallet.
		/// </remarks>
		event Action<WalletInfo> WalletDisconnected;
	}

}