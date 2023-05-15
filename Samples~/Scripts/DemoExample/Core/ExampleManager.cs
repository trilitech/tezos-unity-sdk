using System;
using System.Collections.Generic;
using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;
using Netezos.Encoding;
using Scripts.Helpers;
using Scripts.Tezos;
using UnityEngine;
using Logger = Helpers.Logger;

public class ExampleManager : IExampleManager
{
    private string _networkRPC;

    private const string
        contractAddress = "KT1WguzxyLmuKbJhz3jNuoRzzaUCncfp6PFE"; //"KT1DMWAeaP6wxKWPFDLGDkB7xUg563852AjD";

    private const int softCurrencyID = 0;

    private ITezos _tezos;

    public User CurrentUser { get; private set; }

    public ExampleManager()
    {
        CurrentUser = null;
    }

    public void Init(Action<bool> callback = null)
    {
        _tezos = TezosSingleton.Instance;
        _networkRPC = TezosConfig.Instance.RpcBaseUrl;
    }

    public void Unpair()
    {
        _tezos.Wallet.Disconnect();
        CurrentUser = null;
    }

    public void FetchInventoryItems(Action<List<IItemModel>> callback)
    {
        var activeWalletAddress = _tezos.Wallet.GetActiveAddress(); // Address to the current active account

        const string entrypoint = "view_items_of";
        var input = new { @string = activeWalletAddress };

        CoroutineRunner.Instance.StartWrappedCoroutine(
            _tezos.API.ReadView(
                contractAddress: contractAddress,
                entrypoint: entrypoint,
                input: input,
                callback: result =>
                {
                    Logger.LogDebug("READING INVENTORY DATA");

                    // deserialize the json data to inventory items
                    CoroutineRunner.Instance.StartWrappedCoroutine(
                        NetezosExtensions.HumanizeValue(
                            val: result,
                            rpcUri: _networkRPC,
                            destination: contractAddress,
                            humanizeEntrypoint: "humanizeInventory",
                            onComplete: (ContractInventoryViewResult[] inventory) =>
                                OnInventoryFetched(inventory, callback))
                    );
                }));
    }

    private void OnInventoryFetched(ContractInventoryViewResult[] inventory, Action<List<IItemModel>> callback)
    {
        var dummyItemList = new List<IItemModel>();
        var owner = _tezos.Wallet.GetActiveAddress();

        if (inventory != null)
        {
            foreach (var item in inventory)
            {
                var id = Convert.ToInt32(item.id);
                var itemType = id == 0 ? 0 : Convert.ToInt32(item.item.itemType) % 4 + 1;
                var amount = id == 0 ? Convert.ToInt32(item.amount) : 1;

                var stats = new StatParams(
                    Convert.ToInt32(item.item.damage),
                    Convert.ToInt32(item.item.armor),
                    Convert.ToInt32(item.item.attackSpeed),
                    Convert.ToInt32(item.item.healthPoints),
                    Convert.ToInt32(item.item.manaPoints)
                );

                dummyItemList.Add(new Item(
                    (ItemType)itemType,
                    "Items/TestItem " + itemType,
                    "Item " + id,
                    stats,
                    amount,
                    id,
                    owner));
            }
        }

        callback?.Invoke(dummyItemList);
    }

    public class ContractItem
    {
        public string damage { get; set; }
        public string armor { get; set; }
        public string attackSpeed { get; set; }
        public string healthPoints { get; set; }
        public string manaPoints { get; set; }
        public string itemType { get; set; }
    }

    public class ContractInventoryViewResult
    {
        public string id { get; set; }
        public string amount { get; set; }
        public ContractItem item { get; set; }
    }

    public class ContractMarketplaceViewResult
    {
        public string id { get; set; }
        public string owner { get; set; }
        public string currency { get; set; }
        public string price { get; set; }
        public ContractItem item { get; set; }
    }

    public void FetchMarketItems(Action<List<IItemModel>> callback)
    {
        const string entrypoint = "view_items_on_market";

        // Prepping parameters for Netezos and for Native is different.
        // Double serialization converts double quotes (") symbol into 'u0002' string
        // var input = "{\"prim\": \"Unit\"}";
        var input = new MichelinePrim
        {
            Prim = PrimType.Unit
        };

        CoroutineRunner.Instance.StartWrappedCoroutine(
            _tezos.API.ReadView(
                contractAddress: contractAddress,
                entrypoint: entrypoint,
                input: input,
                callback: result =>
                {
                    // deserialize the json data to market items
                    CoroutineRunner.Instance.StartWrappedCoroutine(
                        NetezosExtensions.HumanizeValue(
                            val: result,
                            rpcUri: _networkRPC,
                            destination: contractAddress,
                            humanizeEntrypoint: "humanizeMarketplace",
                            onComplete: (ContractMarketplaceViewResult[] market) =>
                                OnMarketplaceFetched(market, callback))
                    );
                }));
    }

    private void OnMarketplaceFetched(ContractMarketplaceViewResult[] market, Action<List<IItemModel>> callback)
    {
        var dummyItemList = new List<IItemModel>();

        if (market != null)
        {
            foreach (var item in market)
            {
                var id = Convert.ToInt32(item.id);
                var itemType = id == 0 ? 0 : Convert.ToInt32(item.item.itemType) % 4 + 1;
                var currency = Convert.ToInt32(item.currency);
                var price = Convert.ToInt32(item.price);
                var owner = item.owner;

                var stats = new StatParams(
                    Convert.ToInt32(item.item.damage),
                    Convert.ToInt32(item.item.armor),
                    Convert.ToInt32(item.item.attackSpeed),
                    Convert.ToInt32(item.item.healthPoints),
                    Convert.ToInt32(item.item.manaPoints)
                );

                dummyItemList.Add(new Item(
                    (ItemType)itemType,
                    "Items/TestItem " + itemType,
                    "Item " + id,
                    stats,
                    price,
                    id,
                    owner));
            }
        }

        callback?.Invoke(dummyItemList);
    }

    public void BuyItem(string owner, int itemID)
    {
        const string entryPoint = "buy";

        var parameter = new MichelinePrim
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>
            {
                new MichelineString(owner),
                new MichelineInt(itemID)
            }
        }.ToJson();

        Debug.Log(contractAddress + " " + entryPoint + parameter);
        _tezos.Wallet.CallContract(contractAddress, entryPoint, parameter, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void MintItem()
    {
        const string entrypoint = "mint";
        const string input = "{\"prim\": \"Unit\"}";

        _tezos.Wallet.CallContract(contractAddress, entrypoint, input, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public User GetCurrentUser()
    {
        return CurrentUser;
    }

    public void GetBalance(Action<ulong> callback)
    {
        var routine = _tezos.GetCurrentWalletBalance(callback);
        CoroutineRunner.Instance.StartWrappedCoroutine(routine);
    }

    public void GetSoftBalance(Action<int> callback)
    {
        GetSoftBalanceRoutine(callback);
    }

    private void GetSoftBalanceRoutine(Action<int> callback)
    {
        var caller = _tezos.Wallet.GetActiveAddress();

        var input = new MichelinePrim
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>
            {
                new MichelineString(caller),
                new MichelineInt(softCurrencyID)
            }
        };

        CoroutineRunner.Instance.StartWrappedCoroutine(
            _tezos.API.ReadView(
                contractAddress: contractAddress,
                entrypoint: "get_balance",
                input: input,
                callback: result =>
                {
                    var intProp = result.GetProperty("int");
                    var intValue = Convert.ToInt32(intProp.ToString());
                    callback(intValue);
                }));
    }

    public void TransferItem(int itemID, int amount, string address)
    {
        Logger.LogDebug(
            $"Transfering item {itemID} from {_tezos.Wallet.GetActiveAddress()} to Address: {address}");

        var sender = _tezos.Wallet.GetActiveAddress();
        const string entrypoint = "transfer";
        var input = "[ { \"prim\": \"Pair\", \"args\": [ { \"string\": \"" + sender +
                    "\" }, [ { \"prim\": \"Pair\", \"args\": [ { \"string\": \"" + address +
                    "\" }, { \"prim\": \"Pair\", \"args\": [ { \"int\": \"" + itemID + "\" }, { \"int\": \"" + amount +
                    "\" } ] } ] } ] ] } ]";

        _tezos.Wallet.CallContract(contractAddress, entrypoint, input, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void AddItemToMarket(int itemID, int price)
    {
        Debug.Log("Adding Item " + itemID + " to Market with the price of " + price);

        const string entryPoint = "addToMarket";

        var parameter = new MichelinePrim()
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>()
            {
                new MichelinePrim()
                {
                    Prim = PrimType.Pair,
                    Args = new List<IMicheline>()
                    {
                        new MichelineInt(0), // (currency ID = 0) represents coins
                        new MichelineInt(price),
                    }
                },
                new MichelineInt(itemID),
            }
        }.ToJson();

        _tezos.Wallet.CallContract(contractAddress, entryPoint, parameter, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void RemoveItemFromMarket(int itemID)
    {
        Debug.Log("Removing Item " + itemID + " from market.");

        const string entryPoint = "removeFromMarket";

        var sender = _tezos.Wallet.GetActiveAddress();
        var parameter = new MichelinePrim()
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>()
            {
                new MichelineString(sender),
                new MichelineInt(itemID)
            }
        }.ToJson();

        _tezos.Wallet.CallContract(contractAddress, entryPoint, parameter, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void GetCoins()
    {
        const string entryPoint = "login";
        const string parameter = "{\"prim\": \"Unit\"}";

        _tezos.Wallet.CallContract(contractAddress, entryPoint, parameter, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void IsItemOnMarket(int itemID, string owner, Action<bool> callback)
    {
        const string entrypoint = "is_item_on_market";

        var input = new MichelinePrim()
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>()
            {
                new MichelineString(owner),
                new MichelineInt(itemID)
            }
        };

        CoroutineRunner.Instance.StartWrappedCoroutine(
            _tezos.API.ReadView(
                contractAddress: contractAddress,
                entrypoint: entrypoint,
                input: input,
                callback: result =>
                {
                    var boolString = result.GetProperty("prim");
                    var boolVal = boolString.GetString() == "True";
                    callback?.Invoke(boolVal);
                }));
    }

    public void RequestSignPayload(SignPayloadType signingType, string payload)
    {
        _tezos.Wallet.RequestSignPayload(signingType, payload);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public bool VerifyPayload(SignPayloadType signingType, string payload)
    {
        return _tezos.Wallet.VerifySignedPayload(signingType, payload);
    }

    public string GetActiveAccountAddress()
    {
        return _tezos.Wallet.GetActiveAddress();
    }

    public void Deeplink()
    {
        _tezos.Wallet.Connect();
    }

    public WalletMessageReceiver GetMessageReceiver()
    {
        return _tezos.Wallet.MessageReceiver;
    }
}