using System.Collections.Generic;
using System;
using BeaconSDK;

public interface IExampleManager
{
    public void Init(Action<bool> callback = null);
    public void Unpair();

    public void GetCoins();
    
    /// <summary>
    /// Returns the currently active user.
    /// </summary>
    /// <returns>The Currently active user</returns>
    public User GetCurrentUser();

    /// <summary>
    /// Retrieves items that should be displayed in the inventory
    /// </summary>
    /// <param name="callback">
    /// callback function that takes a list of IItemModels that should be displayed in Inventory
    /// </param>
    public void FetchInventoryItems(Action<List<IItemModel>> callback);

    /// <summary>
    /// Retrieves items that should be displayed in the market
    /// </summary>
    /// <param name="callback">
    /// callback function that takes a list of IItemModels that should be displayed in Market
    /// </param>
    public void FetchMarketItems(Action<List<IItemModel>> callback);

    /// <summary>
    /// Buys an item
    /// </summary>
    /// <param name="owner">Owner of the item being sold</param>
    /// <param name="itemID">ID of the item that will be bought</param>
    /// <param name="callback">callback takes a bool (true if the transaction is completed successfully)</param>
    public void BuyItem(string owner, int itemID);
    
    /// <summary>
    /// Mints an item
    /// </summary>
    public void MintItem();

    /// <summary>
    /// Get account balance
    /// </summary>
    /// <param name="callback">callback that takes the retrieved balance (int)</param>
    public void GetBalance(Action<ulong> callback);
    
    /// <summary>
    /// Get soft currency balance
    /// </summary>
    /// <param name="callback">callback that takes the retrieved balance (int)</param>
    public void GetSoftBalance(Action<int> callback);
    
    /// <summary>
    /// Returns the address of the current active wallet
    /// </summary>
    /// <returns></returns>
    public string GetActiveAccountAddress();

    public void Deeplink();

    /// <summary>
    /// Transfers an item to an account address
    /// </summary>
    /// <param name="itemID">ID of the item that will be transfered</param>
    /// <param name="amount">amount of the item to be transfered</param>
    /// <param name="address">address of the user that will receive the item</param>
    /// <param name="callback">callback takes a bool (true if the transfer is completed successfully)</param>
    public void TransferItem(int itemID, int amount, string address);

    /// <summary>
    /// Transfers an item from the inventory to the market
    /// </summary>
    /// <param name="itemID">ID of the item that will be added to the market</param>
    /// <param name="price">price of the item on the market</param>
    /// <param name="callback">callback takes a bool (true if the process is completed successfully)</param>
    public void AddItemToMarket(int itemID, int price);

    /// <summary>
    /// Transfers an item from the market to the inventory
    /// </summary>
    /// <param name="itemID">ID of the item that will be transfered</param>
    /// <param name="callback">callback takes a bool (true if the process is completed successfully)</param>
    public void RemoveItemFromMarket(int itemID);

    /// <summary>
    /// Return the Tezos MessageReceiver for using callbacks
    /// </summary>
    public BeaconMessageReceiver GetMessageReceiver();

    /// <summary>
    /// Checks if item is on the marketplace in the blockchain
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="owner"></param>
    /// <param name="callback"></param>
    public void IsItemOnMarket(int itemID, string owner, Action<bool> callback);
    
    /// <summary>
    /// Sends a request to sign a payload
    /// </summary>
    /// <param name="payload"></param>
    void RequestSignPayload(string payload);
    
    bool VerifyPayload(string payload);
}
