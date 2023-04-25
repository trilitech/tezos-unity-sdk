using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;
using TezosAPI.Models;
using TezosAPI.Models.Tokens;

namespace TezosAPI
{
    public interface ITezosAPI
    {
        /// <summary>
        /// Returns the network RPC andress (e.g https://rpc.ghostnet.teztnets.xyz)
        /// </summary 
        public string NetworkRPC { get; }

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
        /// <param name="payload">payload string that is going to be signed</param>
        public void RequestSignPayload(SignPayloadType signingType, string payload);

        /// <summary>
        /// Verify a signed payload to check if it is valid
        /// </summary>
        /// <param name="payload">payload string that is going to be signed</param>
        public bool VerifySignedPayload(SignPayloadType signingType, string payload);

        /// <summary>
        /// Exposes a Monobehaviour class that exposes wallet events
        /// </summary>
        public BeaconMessageReceiver MessageReceiver { get; }

        // Gets all tokens currently owned by a given address.
        public IEnumerator GetTokensForOwner(
            Action<IEnumerable<TokenBalance>> cb,
            string owner,
            bool withMetadata,
            long maxItems,
            TokensForOwnerOrder orderBy
        );
    }
}