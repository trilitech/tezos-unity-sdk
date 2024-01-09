using System;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using Netezos.Encoding;
using TezosSDK.Helpers;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models.Tokens;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;
using Random = System.Random;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.Core
{

	public class ExampleManager : IExampleManager
	{
		private const string
			contractAddress = "KT1WguzxyLmuKbJhz3jNuoRzzaUCncfp6PFE"; //"KT1DMWAeaP6wxKWPFDLGDkB7xUg563852AjD";

		private const int softCurrencyID = 0;
		private string _networkRPC;

		public ExampleManager()
		{
			CurrentUser = null;
		}

		public User CurrentUser { get; private set; }

		public ITezos Tezos { get; private set; }

		public void Init(Action<bool> callback = null)
		{
			Tezos = TezosManager.Instance.Tezos;
			_networkRPC = TezosManager.Instance.Config.Rpc;
		}

		public void Unpair()
		{
			Tezos.Wallet.Disconnect();
			CurrentUser = null;
		}

		public void FetchInventoryItems(Action<List<IItemModel>> callback)
		{
			var activeWalletAddress = Tezos.Wallet.GetWalletAddress(); // Address to the current active account

			const string entrypoint = "view_items_of";

			var input = new
			{
				@string = activeWalletAddress
			};

			CoroutineRunner.Instance.StartWrappedCoroutine(Tezos.API.ReadView(contractAddress, entrypoint,
				JsonSerializer.Serialize(input, JsonOptions.DefaultOptions), result =>
				{
					Logger.LogDebug("READING INVENTORY DATA");

					// deserialize the json data to inventory items
					CoroutineRunner.Instance.StartWrappedCoroutine(NetezosExtensions.HumanizeValue(result, _networkRPC,
						contractAddress, "humanizeInventory",
						(ContractInventoryViewResult[] inventory) => OnInventoryFetched(inventory, callback)));
				}));
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
			}.ToJson();

			CoroutineRunner.Instance.StartWrappedCoroutine(Tezos.API.ReadView(contractAddress, entrypoint, input,
				result =>
				{
					// deserialize the json data to market items
					CoroutineRunner.Instance.StartWrappedCoroutine(NetezosExtensions.HumanizeValue(result, _networkRPC,
						contractAddress, "humanizeMarketplace",
						(ContractMarketplaceViewResult[] market) => OnMarketplaceFetched(market, callback)));
				}));
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

			Logger.LogDebug(contractAddress + " " + entryPoint + parameter);
			Tezos.Wallet.CallContract(contractAddress, entryPoint, parameter);

#if UNITY_IOS || UNITY_ANDROID
            Application.OpenURL("tezos://");
#endif
		}

		public void MintItem()
		{
			const string entrypoint = "mint";
			const string input = "{\"prim\": \"Unit\"}";

			Tezos.Wallet.CallContract(contractAddress, entrypoint, input);

#if UNITY_IOS || UNITY_ANDROID
            Application.OpenURL("tezos://");
#endif
		}

		public void MintFA2(Action<TokenBalance> callback)
		{
			var rnd = new Random();
			var randomInt = rnd.Next(1, int.MaxValue);
			var randomAmount = rnd.Next(1, 1024);

			var activeAccount = Tezos.Wallet.GetWalletAddress();
			const string imageAddress = "ipfs://QmX4t8ikQgjvLdqTtL51v6iVun9tNE7y7Txiw4piGQVNgK";

			var metadata = new TokenMetadata
			{
				Name = $"testName_{randomInt}",
				Description = $"testDescription_{randomInt}",
				Symbol = $"TST_{randomInt}",
				Decimals = "0",
				DisplayUri = imageAddress,
				ArtifactUri = imageAddress,
				ThumbnailUri = imageAddress
			};

			Tezos.TokenContract.Mint(callback, metadata, activeAccount, randomAmount);

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
			var routine = Tezos.GetCurrentWalletBalance(callback);
			CoroutineRunner.Instance.StartWrappedCoroutine(routine);
		}

		public void GetSoftBalance(Action<int> callback)
		{
			GetSoftBalanceRoutine(callback);
		}

		public void TransferItem(int itemID, int amount, string address)
		{
			Logger.LogDebug($"Transfering item {itemID} from {Tezos.Wallet.GetWalletAddress()} to Address: {address}");

			var sender = Tezos.Wallet.GetWalletAddress();
			const string entrypoint = "transfer";

			var input = "[ { \"prim\": \"Pair\", \"args\": [ { \"string\": \"" + sender +
			            "\" }, [ { \"prim\": \"Pair\", \"args\": [ { \"string\": \"" + address +
			            "\" }, { \"prim\": \"Pair\", \"args\": [ { \"int\": \"" + itemID + "\" }, { \"int\": \"" +
			            amount + "\" } ] } ] } ] ] } ]";

			Tezos.Wallet.CallContract(contractAddress, entrypoint, input);

#if UNITY_IOS || UNITY_ANDROID
            Application.OpenURL("tezos://");
#endif
		}

		public void AddItemToMarket(int itemID, int price)
		{
			Debug.Log("Adding Item " + itemID + " to Market with the price of " + price);

			const string entryPoint = "addToMarket";

			var parameter = new MichelinePrim
			{
				Prim = PrimType.Pair,
				Args = new List<IMicheline>
				{
					new MichelinePrim
					{
						Prim = PrimType.Pair,
						Args = new List<IMicheline>
						{
							new MichelineInt(0), // (currency ID = 0) represents coins
							new MichelineInt(price)
						}
					},
					new MichelineInt(itemID)
				}
			}.ToJson();

			Tezos.Wallet.CallContract(contractAddress, entryPoint, parameter);

#if UNITY_IOS || UNITY_ANDROID
            Application.OpenURL("tezos://");
#endif
		}

		public void RemoveItemFromMarket(int itemID)
		{
			Debug.Log("Removing Item " + itemID + " from market.");

			const string entryPoint = "removeFromMarket";

			var sender = Tezos.Wallet.GetWalletAddress();

			var parameter = new MichelinePrim
			{
				Prim = PrimType.Pair,
				Args = new List<IMicheline>
				{
					new MichelineString(sender),
					new MichelineInt(itemID)
				}
			}.ToJson();

			Tezos.Wallet.CallContract(contractAddress, entryPoint, parameter);

#if UNITY_IOS || UNITY_ANDROID
            Application.OpenURL("tezos://");
#endif
		}

		public void DeployContract(Action<string> deployedContractAddress)
		{
			Tezos.TokenContract.Deploy(deployedContractAddress);
		}

		public void ChangeContract(string activeContractAddress)
		{
			PlayerPrefs.SetString("CurrentContract:" + Tezos.Wallet.GetWalletAddress(), activeContractAddress);
			Tezos.TokenContract.Address = activeContractAddress;
		}

		public void GetCoins()
		{
			const string entryPoint = "login";
			const string parameter = "{\"prim\": \"Unit\"}";

			Tezos.Wallet.CallContract(contractAddress, entryPoint, parameter);

#if UNITY_IOS || UNITY_ANDROID
            Application.OpenURL("tezos://");
#endif
		}

		public void IsItemOnMarket(int itemID, string owner, Action<bool> callback)
		{
			const string entrypoint = "is_item_on_market";

			var input = new MichelinePrim
			{
				Prim = PrimType.Pair,
				Args = new List<IMicheline>
				{
					new MichelineString(owner),
					new MichelineInt(itemID)
				}
			}.ToJson();

			CoroutineRunner.Instance.StartWrappedCoroutine(Tezos.API.ReadView(contractAddress, entrypoint, input,
				result =>
				{
					var boolString = result.GetProperty("prim");
					var boolVal = boolString.GetString() == "True";
					callback?.Invoke(boolVal);
				}));
		}

		public void RequestSignPayload(SignPayloadType signingType, string payload)
		{
			Tezos.Wallet.RequestSignPayload(signingType, payload);

#if UNITY_IOS || UNITY_ANDROID
            Application.OpenURL("tezos://");
#endif
		}

		public bool VerifyPayload(SignPayloadType signingType, string payload)
		{
			return Tezos.Wallet.VerifySignedPayload(signingType, payload);
		}

		public string GetActiveAccountAddress()
		{
			return Tezos.Wallet.GetWalletAddress();
		}

		public void Login(WalletProviderType walletProvider)
		{
			Tezos.Wallet.Connect(walletProvider);
		}

		public IWalletEventManager GetWalletMessageReceiver()
		{
			return Tezos.Wallet.EventManager;
		}

		public void GetOriginatedContracts(Action<IEnumerable<TokenContract>> callback)
		{
			var routine = Tezos.GetOriginatedContracts(callback);
			CoroutineRunner.Instance.StartWrappedCoroutine(routine);
		}

		public void OnReady()
		{
		}

		private void OnInventoryFetched(ContractInventoryViewResult[] inventory, Action<List<IItemModel>> callback)
		{
			var dummyItemList = new List<IItemModel>();
			var owner = Tezos.Wallet.GetWalletAddress();

			if (inventory != null)
			{
				foreach (var item in inventory)
				{
					var id = Convert.ToInt32(item.id);
					var itemType = id == 0 ? 0 : Convert.ToInt32(item.item.itemType) % 4 + 1;
					var amount = id == 0 ? Convert.ToInt32(item.amount) : 1;

					var stats = new StatParams(Convert.ToInt32(item.item.damage), Convert.ToInt32(item.item.armor),
						Convert.ToInt32(item.item.attackSpeed), Convert.ToInt32(item.item.healthPoints),
						Convert.ToInt32(item.item.manaPoints));

					dummyItemList.Add(new Item((ItemType)itemType, "Items/TestItem " + itemType, "Item " + id, stats,
						amount, id, owner));
				}
			}

			callback?.Invoke(dummyItemList);
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

					var stats = new StatParams(Convert.ToInt32(item.item.damage), Convert.ToInt32(item.item.armor),
						Convert.ToInt32(item.item.attackSpeed), Convert.ToInt32(item.item.healthPoints),
						Convert.ToInt32(item.item.manaPoints));

					dummyItemList.Add(new Item((ItemType)itemType, "Items/TestItem " + itemType, "Item " + id, stats,
						price, id, owner));
				}
			}

			callback?.Invoke(dummyItemList);
		}

		private void GetSoftBalanceRoutine(Action<int> callback)
		{
			var caller = Tezos.Wallet.GetWalletAddress();

			var input = new MichelinePrim
			{
				Prim = PrimType.Pair,
				Args = new List<IMicheline>
				{
					new MichelineString(caller),
					new MichelineInt(softCurrencyID)
				}
			}.ToJson();

			CoroutineRunner.Instance.StartWrappedCoroutine(Tezos.API.ReadView(contractAddress, "get_balance", input,
				result =>
				{
					var intProp = result.GetProperty("int");
					var intValue = Convert.ToInt32(intProp.ToString());
					callback(intValue);
				}));
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
	}

}