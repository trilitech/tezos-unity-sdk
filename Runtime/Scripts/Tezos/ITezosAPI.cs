using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Beacon;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using TezosSDK.Tezos.Wallet;

namespace TezosSDK.Tezos
{
    [Obsolete("ITezosAPI will be renamed to ITezos in future versions, please use ITezos type instead")]
    public interface ITezosAPI : ITezos
    {
        /// <summary>
        /// Exposes a MonoBehaviour class that exposes wallet events
        /// </summary>
        [Obsolete("BeaconMessageReceiver is deprecated, please use ITezos.IWalletProvider.WalletMessageReceiver instead")]
        BeaconMessageReceiver MessageReceiver { get; }

        /// <summary>
        /// Makes a call to connect with a wallet
        /// <param name="withRedirectToWallet">Should we open wallet app on mobiles after connect?</param>
        /// </summary>
        [Obsolete("ConnectWallet is deprecated, please use ITezos.IWalletProvider.Connect instead")]
        void ConnectWallet(WalletProviderType walletProvider, bool withRedirectToWallet = true);

        /// <summary>
        /// Unpair with wallet and disconnect
        /// </summary>
        [Obsolete("DisconnectWallet is deprecated, please use ITezos.IWalletProvider.Disconnect instead")]
        void DisconnectWallet();

        /// <summary>
        /// Returns the address of the current active wallet
        /// </summary>
        /// <returns></returns>
        [Obsolete("GetActiveWalletAddress is deprecated, please use ITezos.IWalletProvider.GetActiveAddress instead")]
        string GetActiveWalletAddress();

        /// <summary>
        /// Sends a request to the sign a payload string
        /// </summary>
        /// <param name="signingType">type of payload: raw, operation or micheline</param>
        /// <param name="payload">payload string that is going to be signed</param>
        [Obsolete("RequestSignPayload is deprecated, please use ITezos.IWalletProvider.RequestSignPayload instead")]
        void RequestSignPayload(SignPayloadType signingType, string payload);

        /// <summary>
        /// Verify a signed payload to check if it is valid
        /// </summary>
        /// <param name="signingType">type of payload: raw, operation or micheline</param>
        /// <param name="payload">payload string that is going to be signed</param>
        [Obsolete("VerifySignedPayload is deprecated, please use ITezos.IWalletProvider.VerifySignedPayload instead")]
        bool VerifySignedPayload(SignPayloadType signingType, string payload);

        /// <summary>
        /// Performs an operation in the contract
        /// </summary>
        /// <param name="contractAddress">destination address of the smart contract</param>
        /// <param name="entryPoint">entry point used in the smart contract</param>
        /// <param name="input">parameters called on the entry point</param>
        /// <param name="amount">amount of Tez sent into the contract</param>
        [Obsolete("CallContract is deprecated, please use ITezos.IWalletProvider.CallContract instead")]
        void CallContract(
            string contractAddress,
            string entryPoint,
            string input,
            ulong amount = 0);

        /// <summary>
        /// Fetch current wallet Tezos balance in micro tez
        /// </summary>
        /// <param name="callback">callback action that runs with the ulong balance is fetched</param>
        /// <returns></returns>
        [Obsolete("ReadBalance is deprecated, please use ITezos.GetCurrentWalletBalance instead")]
        IEnumerator ReadBalance(Action<ulong> callback);

        /// <summary>
        /// Reading data from a contract view
        /// </summary>
        /// <param name="contractAddress">destination address of the smart contract</param>
        /// <param name="entrypoint">entry point used in the smart contract</param>
        /// <param name="input">parameters called on the entry point</param>
        /// <param name="callback">callback action that runs with the json data is fetched</param>
        /// <returns></returns>
        [Obsolete("ReadView is deprecated, please use ITezos.ITezosDataAPI.ReadView instead")]
        public IEnumerator ReadView(
            string contractAddress,
            string entrypoint,
            object input,
            Action<JsonElement> callback);

        // Gets all tokens currently owned by a given address.
        [Obsolete("GetTokensForOwner is deprecated, please use ITezos.ITezosDataAPI.GetTokensForOwner instead")]
        public IEnumerator GetTokensForOwner(
            Action<IEnumerable<TokenBalance>> callback,
            string owner,
            bool withMetadata,
            long maxItems,
            TokensForOwnerOrder orderBy);

        // Get the owner(s) for a token.
        [Obsolete("GetOwnersForToken is deprecated, please use ITezos.ITezosDataAPI.GetOwnersForToken instead")]
        public IEnumerator GetOwnersForToken(
            Action<IEnumerable<TokenBalance>> callback,
            string contractAddress,
            uint tokenId,
            long maxItems,
            OwnersForTokenOrder orderBy);

        // Gets all owners for a given token contract.
        [Obsolete("GetOwnersForContract is deprecated, please use ITezos.ITezosDataAPI.GetOwnersForContract instead")]
        public IEnumerator GetOwnersForContract(
            Action<IEnumerable<TokenBalance>> callback,
            string contractAddress,
            long maxItems,
            OwnersForContractOrder orderBy);

        // Checks whether a wallet holds a token in a given contract.
        [Obsolete("IsHolderOfContract is deprecated, please use ITezos.ITezosDataAPI.IsHolderOfContract instead")]
        public IEnumerator IsHolderOfContract(
            Action<bool> callback,
            string wallet,
            string contractAddress);

        // Checks whether a wallet holds a particular token.
        [Obsolete("IsHolderOfToken is deprecated, please use ITezos.ITezosDataAPI.IsHolderOfToken instead")]
        public IEnumerator IsHolderOfToken(
            Action<bool> callback,
            string wallet,
            string contractAddress,
            uint tokenId);

        // Gets the metadata associated with a given token.
        [Obsolete("GetTokenMetadata is deprecated, please use ITezos.ITezosDataAPI.GetTokenMetadata instead")]
        public IEnumerator GetTokenMetadata(
            Action<JsonElement> callback,
            string contractAddress,
            uint tokenId);

        // Queries token high-level collection/contract level information.
        [Obsolete("GetContractMetadata is deprecated, please use ITezos.ITezosDataAPI.GetContractMetadata instead")]
        public IEnumerator GetContractMetadata(
            Action<JsonElement> callback,
            string contractAddress);

        // Gets all tokens for a given token contract.
        [Obsolete("GetTokensForContract is deprecated, please use ITezos.ITezosDataAPI.GetTokensForContract instead")]
        public IEnumerator GetTokensForContract(
            Action<IEnumerable<Token>> callback,
            string contractAddress,
            bool withMetadata,
            long maxItems,
            TokensForContractOrder orderBy);

        // Returns operation status: true if applied, false if failed, null (or HTTP 204) if doesn't exist.
        [Obsolete("GetOperationStatus is deprecated, please use ITezos.ITezosDataAPI.GetOperationStatus instead")]
        public IEnumerator GetOperationStatus(
            Action<bool?> callback,
            string operationHash);
    }

    public interface ITezos
    {
        /// <summary>
        /// Tezos chain data source
        /// </summary>
        ITezosDataAPI API { get; }

        /// <summary>
        /// Wallet features
        /// </summary>
        IWalletProvider Wallet { get; }

        /// <summary>
        /// Fetch current wallet Tezos balance in micro tez
        /// </summary>
        /// <param name="callback">callback action that runs with the ulong balance is fetched</param>
        /// <returns></returns>
        IEnumerator GetCurrentWalletBalance(Action<ulong> callback);
    }
}