using System;
using System.Collections.Generic;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models.Tokens;
using TezosSDK.Tezos.Wallet;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.Core
{

	public interface IExampleManager
	{
		ITezos Tezos { get; }

		public void Init(Action<bool> callback = null);
		public void Unpair();

		public void GetCoins();

        /// <summary>
        ///     Returns the currently active user.
        /// </summary>
        /// <returns>The Currently active user</returns>
        public User GetCurrentUser();

        /// <summary>
        ///     Retrieves items that should be displayed in the inventory
        /// </summary>
        /// <param name="callback">
        ///     callback function that takes a list of IItemModels that should be displayed in Inventory
        /// </param>
        public void FetchInventoryItems(Action<List<IItemModel>> callback);

        /// <summary>
        ///     Retrieves items that should be displayed in the market
        /// </summary>
        /// <param name="callback">
        ///     callback function that takes a list of IItemModels that should be displayed in Market
        /// </param>
        public void FetchMarketItems(Action<List<IItemModel>> callback);

        /// <summary>
        ///     Buys an item
        /// </summary>
        /// <param name="owner">Owner of the item being sold</param>
        /// <param name="itemID">ID of the item that will be bought</param>
        /// <param name="callback">callback takes a bool (true if the transaction is completed successfully)</param>
        public void BuyItem(string owner, int itemID);

        /// <summary>
        ///     Mints an item
        /// </summary>
        public void MintItem();

        /// <summary>
        ///     Mints FA2 Token
        /// </summary>
        public void MintFA2(Action<TokenBalance> callback);

        /// <summary>
        ///     Get account balance
        /// </summary>
        /// <param name="callback">callback that takes the retrieved balance (int)</param>
        public void GetBalance(Action<ulong> callback);

        /// <summary>
        ///     Get soft currency balance
        /// </summary>
        /// <param name="callback">callback that takes the retrieved balance (int)</param>
        public void GetSoftBalance(Action<int> callback);

        /// <summary>
        ///     Returns the address of the current active wallet
        /// </summary>
        /// <returns></returns>
        public string GetActiveAccountAddress();

		public void Login(WalletProviderType walletProvider);

        /// <summary>
        ///     Transfers an item to an account address
        /// </summary>
        /// <param name="itemID">ID of the item that will be transfered</param>
        /// <param name="amount">amount of the item to be transfered</param>
        /// <param name="address">address of the user that will receive the item</param>
        /// <param name="callback">callback takes a bool (true if the transfer is completed successfully)</param>
        public void TransferItem(int itemID, int amount, string address);

        /// <summary>
        ///     Transfers an item from the inventory to the market
        /// </summary>
        /// <param name="itemID">ID of the item that will be added to the market</param>
        /// <param name="price">price of the item on the market</param>
        /// <param name="callback">callback takes a bool (true if the process is completed successfully)</param>
        public void AddItemToMarket(int itemID, int price);

        /// <summary>
        ///     Transfers an item from the market to the inventory
        /// </summary>
        /// <param name="itemID">ID of the item that will be transfered</param>
        /// <param name="callback">callback takes a bool (true if the process is completed successfully)</param>
        public void RemoveItemFromMarket(int itemID);

        /// <summary>
        ///     Return the Tezos wallet MessageReceiver for using callbacks
        /// </summary>
        public IWalletEventManager GetWalletMessageReceiver();

        /// <summary>
        ///     Checks if item is on the marketplace in the blockchain
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        public void IsItemOnMarket(int itemID, string owner, Action<bool> callback);

        /// <summary>
        ///     Sends a request to sign a payload
        /// </summary>
        /// <param name="signingType"></param>
        /// <param name="payload"></param>
        void RequestSignPayload(SignPayloadType signingType, string payload);

		bool VerifyPayload(SignPayloadType signingType, string payload);

        /// <summary>
        ///     Deploy FA2 contract
        /// </summary>
        void DeployContract(Action<string> deployedContractAddress);

        /// <summary>
        ///     Switch FA2 contract
        /// </summary>
        void ChangeContract(string activeContractAddress);

        /// <summary>
        ///     Return originated contracts by account for using callbacks
        /// </summary>
        /// <param name="callback">callback that takes the retrieved contracts(IEnumerable)</param>
        void GetOriginatedContracts(Action<IEnumerable<TokenContract>> callback);

        /// <summary>
        ///     Callback that needed in WebGL to determine that UI is rendered
        /// </summary>
        void OnReady();
	}

}