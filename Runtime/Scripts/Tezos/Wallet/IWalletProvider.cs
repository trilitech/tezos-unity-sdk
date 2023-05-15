using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;

namespace Scripts.Tezos.Wallet
{
    public interface IWalletProvider
    {
        /// <summary>
        /// Exposes a MonoBehaviour class that exposes wallet events
        /// </summary>
        WalletMessageReceiver MessageReceiver { get; }

        /// <summary>
        /// Makes a call to connect with a wallet
        /// </summary>
        void Connect();

        /// <summary>
        /// Unpair with wallet and disconnect
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Returns the address of the current active wallet
        /// </summary>
        /// <returns></returns>
        string GetActiveAddress();

        /// <summary>
        /// Sends a request to the sign a payload string
        /// </summary>
        /// <param name="signingType">type of payload: raw, operation or micheline</param>
        /// <param name="payload">payload string that is going to be signed</param>
        void RequestSignPayload(SignPayloadType signingType, string payload);

        /// <summary>
        /// Verify a signed payload to check if it is valid
        /// </summary>
        /// <param name="signingType">type of payload: raw, operation or micheline</param>
        /// <param name="payload">payload string that is going to be signed</param>
        bool VerifySignedPayload(SignPayloadType signingType, string payload);

        /// <summary>
        /// Performs an operation in the contract
        /// </summary>
        /// <param name="contractAddress">destination address of the smart contract</param>
        /// <param name="entryPoint">entry point used in the smart contract</param>
        /// <param name="input">parameters called on the entry point</param>
        /// <param name="amount">amount of Tez sent into the contract</param>
        void CallContract(
            string contractAddress,
            string entryPoint,
            string input,
            ulong amount = 0);
    }
}