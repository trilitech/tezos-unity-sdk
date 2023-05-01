using System;
using System.Collections.Generic;
using Beacon.Sdk.Beacon.Sign;
using BeaconSDK;
using Netezos.Encoding;
using UnityEngine;
using TezosAPI;

public class ExampleManager : IExampleManager
{
    private string _networkRPC;
    private const string contractAddress = "KT1WguzxyLmuKbJhz3jNuoRzzaUCncfp6PFE";//"KT1DMWAeaP6wxKWPFDLGDkB7xUg563852AjD";
    private const int softCurrencyID = 0;

    private User _currentUser;

    public User CurrentUser => _currentUser;

    public ExampleManager()
    {
        _currentUser = null;
    }

    public void Init(Action<bool> callback = null)
    {
        _networkRPC = TezosSingleton.Instance.NetworkRPC;
    }

    public void Unpair()
    {
        TezosSingleton.Instance.DisconnectWallet();
        _currentUser = null;
    }

    public void FetchInventoryItems(Action<List<IItemModel>> callback)
    {
        var sender = TezosSingleton.Instance.GetActiveWalletAddress(); // Address to the current active account

        var destination = contractAddress; // our temporary inventory contract
        var entrypoint = "view_items_of";
        var input = new { @string = sender };

        CoroutineRunner.Instance.StartCoroutine(
            TezosSingleton.Instance.ReadView(contractAddress, entrypoint, input, result =>
            {
                Debug.Log("READING INVENTORY DATA");
                // deserialize the json data to inventory items
                CoroutineRunner.Instance.StartCoroutine(
                    BeaconSDK.NetezosExtensions.HumanizeValue(result, _networkRPC, destination, "humanizeInventory",
                        (ContractInventoryViewResult[] inventory) => OnInventoryFetched(inventory, callback))
                );
            }));
    }

    private void OnInventoryFetched(ContractInventoryViewResult[] inventory, Action<List<IItemModel>> callback)
    {
        List<IItemModel> dummyItemList = new List<IItemModel>();

        var owner = TezosSingleton.Instance.GetActiveWalletAddress();

        if (inventory != null)
            foreach (var item in inventory)
            {
                var id = Convert.ToInt32(item.id);
                var itemType = id == 0 ? 0 : Convert.ToInt32(item.item.itemType) % 4 + 1;
                int amount = id == 0 ? Convert.ToInt32(item.amount) : 1;

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
        var destination = contractAddress;
        var entrypoint = "view_items_on_market";

        // Prepping parameters for Netezos and for Native is different.
        // Double serialization converts double quotes (") symbol into 'u0002' string
        // var input = "{\"prim\": \"Unit\"}";
        var input = new MichelinePrim
        {
            Prim = PrimType.Unit
        };

        CoroutineRunner.Instance.StartCoroutine(
            TezosSingleton.Instance.ReadView(contractAddress, entrypoint, input, result =>
            {
                // deserialize the json data to market items
                CoroutineRunner.Instance.StartCoroutine(
                    BeaconSDK.NetezosExtensions.HumanizeValue(result, _networkRPC, destination, "humanizeMarketplace",
                        (ContractMarketplaceViewResult[] market) => OnMarketplaceFetched(market, callback))
                    );
            }));
    }

    private void OnMarketplaceFetched(ContractMarketplaceViewResult[] market, Action<List<IItemModel>> callback)
    {
        List<IItemModel> dummyItemList = new List<IItemModel>();

        if (market != null)
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

        callback?.Invoke(dummyItemList);
    }

    public void BuyItem(string owner, int itemID)
    {
        var destination = contractAddress;
        var entryPoint = "buy";

        var parameter = new MichelinePrim
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>
            {
                new MichelineString(owner),
                new MichelineInt(itemID)
            }
        }.ToJson();

        Debug.Log(destination + " " + entryPoint + parameter);
        TezosSingleton.Instance.CallContract(contractAddress, entryPoint, parameter, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void MintItem()
    {
        var destination = contractAddress;
        var entrypoint = "mint";
        var input = "{\"prim\": \"Unit\"}";

        TezosSingleton.Instance.CallContract(contractAddress, entrypoint, input, 0);

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
        var routine = TezosSingleton.Instance.ReadBalance(callback);
        CoroutineRunner.Instance.StartCoroutine(routine);
    }

    public void GetSoftBalance(Action<int> callback)
    {
        GetSoftBalanceRoutine(callback);
    }

    private void GetSoftBalanceRoutine(Action<int> callback)
    {
        var caller = TezosSingleton.Instance.GetActiveWalletAddress();

        var input = new MichelinePrim()
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>()
            {
                new MichelineString(caller),
                new MichelineInt(softCurrencyID)
            }
        };

        CoroutineRunner.Instance.StartCoroutine(
            TezosSingleton.Instance.ReadView(contractAddress, "get_balance", input, result =>
            {
                var intProp = result.GetProperty("int");
                var intValue = Convert.ToInt32(intProp.ToString());
                callback(intValue);
            }));
    }

    public void TransferItem(int itemID, int amount, string address)
    {
        Debug.Log("Transfering Item " + itemID + " from " + TezosSingleton.Instance.GetActiveWalletAddress() + " to Address: " + address);

        var destination = contractAddress; // our temporary inventory contract
        var sender = TezosSingleton.Instance.GetActiveWalletAddress();
        var entrypoint = "transfer";
        var input = "[ { \"prim\": \"Pair\", \"args\": [ { \"string\": \"" + sender + "\" }, [ { \"prim\": \"Pair\", \"args\": [ { \"string\": \"" + address + "\" }, { \"prim\": \"Pair\", \"args\": [ { \"int\": \"" + itemID + "\" }, { \"int\": \"" + amount + "\" } ] } ] } ] ] } ]";

        TezosSingleton.Instance.CallContract(contractAddress, entrypoint, input, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void AddItemToMarket(int itemID, int price)
    {
        Debug.Log("Adding Item " + itemID + " to Market with the price of " + price);

        var entryPoint = "addToMarket";

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

        TezosSingleton.Instance.CallContract(contractAddress, entryPoint, parameter, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void RemoveItemFromMarket(int itemID)
    {
        Debug.Log("Removing Item " + itemID + " from market.");

        var destination = contractAddress;
        var entryPoint = "removeFromMarket";

        var sender = TezosSingleton.Instance.GetActiveWalletAddress();
        var parameter = new MichelinePrim()
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>()
            {
                new MichelineString(sender),
                new MichelineInt(itemID)
            }
        }.ToJson();

        TezosSingleton.Instance.CallContract(contractAddress, entryPoint, parameter, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void GetCoins()
    {
        var destination = contractAddress;
        var entryPoint = "login";
        var parameter = "{\"prim\": \"Unit\"}";

        TezosSingleton.Instance.CallContract(contractAddress, entryPoint, parameter, 0);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public void IsItemOnMarket(int itemID, string owner, Action<bool> callback)
    {
        var entrypoint = "is_item_on_market";

        var input = new MichelinePrim()
        {
            Prim = PrimType.Pair,
            Args = new List<IMicheline>()
            {
                new MichelineString(owner),
                new MichelineInt(itemID)
            }
        };

        CoroutineRunner.Instance.StartCoroutine(
            TezosSingleton.Instance.ReadView(contractAddress, entrypoint, input, result =>
            {
                var boolString = result.GetProperty("prim");
                var boolVal = boolString.GetString() == "True";

                callback?.Invoke(boolVal);
            }));
    }

    public void RequestSignPayload(SignPayloadType signingType, string payload)
    {
        TezosSingleton.Instance.RequestSignPayload(signingType, payload);

#if UNITY_IOS || UNITY_ANDROID
        Application.OpenURL("tezos://");
#endif
    }

    public bool VerifyPayload(SignPayloadType signingType, string payload)
    {
        return TezosSingleton.Instance.VerifySignedPayload(signingType, payload);
    }

    public string GetActiveAccountAddress()
    {
        return TezosSingleton.Instance.GetActiveWalletAddress();
    }

    public void Deeplink()
    {
        TezosSingleton.Instance.ConnectWallet();
    }

    public BeaconMessageReceiver GetMessageReceiver()
    {
        return TezosSingleton.Instance.MessageReceiver;
    }
}
