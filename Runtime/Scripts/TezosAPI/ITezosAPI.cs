using System;
using System.Collections;
using System.Text.Json;
using BeaconSDK;

namespace TezosAPI
{
    public interface ITezosAPI
    {
        /// <summary>
        /// Makes a call to connect with a wallet (e.g. Temple Wallet)
        /// Works for iOS and Android builds
        /// </summary>
        public void ConnectWallet();

        /// <summary>
        /// Unpair with wallet and disconnect
        /// </summary>
        public void DisconnectWallet();

        /// <summary>
        /// Returns the address of the current active wallet
        /// </summary>
        /// <returns></returns>
        public string GetActiveWalletAddress();

        /// <summary>
        /// An IEnumerator for reading the account's balance
        /// Can be called in a StartCoroutine()
        /// </summary>
        /// <param name="callback">callback action that runs with the float balance is fetched</param>
        /// <returns></returns>
        public IEnumerator ReadBalance(Action<ulong> callback);

        /// <summary>
        /// An IEnumerator for reading data from a contract view
        /// Can be called in a StartCoroutine()
        /// </summary>
        /// <param name="contractAddress">destination address of the smart contract</param>
        /// <param name="entryPoint">entry point used in the smart contract</param>
        /// <param name="input">parameters called on the entry point</param>
        /// <param name="callback">callback action that runs with the json data is fetched</param>
        /// <returns></returns>
        public IEnumerator ReadView(string contractAddress, string entryPoint, object input,
            Action<JsonElement> callback);

        /// <summary>
        /// Performs an operation in the contract
        /// </summary>
        /// <param name="contractAddress">destination address of the smart contract</param>
        /// <param name="entryPoint">entry point used in the smart contract</param>
        /// <param name="input">parameters called on the entry point</param>
        /// <param name="amount">amount of Tez sent into the contract</param>
        public void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0);
        
        /// <summary>
        /// Sends a permission request to the blockchain network
        /// </summary>
        public void RequestPermission();

        /// <summary>
        /// Sends a request to the sign a payload string
        /// </summary>
        /// <param name="signingType">enumerates the signing type (0 = MICHELINE, 1 = OPERATION, 2 = RAW)</param>
        /// <param name="payload">payload string that is going to be signed</param>
        public void RequestSignPayload(int signingType, string payload);

        /// <summary>
        /// Exposes a Monobehaviour class that exposes wallet events
        /// </summary>
        public BeaconMessageReceiver MessageReceiver { get; }
    }
}
