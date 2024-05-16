using System;
using System.Collections.Generic;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using Netezos.Encoding;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Helpers.Json;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.Interfaces;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Models.Tokens;
using TezosSDK.WalletServices.Interfaces;
using UnityEngine;
using Random = System.Random;
using Logger = TezosSDK.Helpers.Logging.Logger;

namespace TezosSDK.Samples.MarketplaceSample.MarketplaceExample.Core
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
			Tezos.WalletConnection.Disconnect();
			CurrentUser = null;
		}

		public void FetchInventoryItems(Action<List<IItemModel>> callback)
		{
			var activeWalletAddress = Tezos.WalletAccount.GetWalletAddress(); // Address to the current active account

			const string entrypoint = "view_items_of";

			var input = new
			{
				@string = activeWalletAddress
			};

			CoroutineRunner.Instance.StartWrappedCoroutine(Tezos.API.ReadView(contractAddress, entrypoint,
				SerializeInput(input), readViewResult =>
				{
					if (readViewResult.Success)
					{
						Logger.LogDebug("READING INVENTORY DATA");
						// Start another coroutine to process the result
						ProcessInventoryResult(readViewResult.Data, callback);
					}
					else
					{
						// Handle errors
						Logger.LogError("Error fetching inventory: " + readViewResult.ErrorMessage);
					}
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
					CoroutineRunner.Instance.StartWrappedCoroutine(NetezosExtensions.HumanizeValue(result.Data,
						_networkRPC, contractAddress, "humanizeMarketplace",
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
			Tezos.WalletTransaction.CallContract(contractAddress, entryPoint, parameter);

#if UNITY_IOS || UNITY_ANDROID
			Application.OpenURL("tezos://");
#endif
		}

		public void MintItem()
		{
			const string entrypoint = "mint";
			const string input = "{\"prim\": \"Unit\"}";

			Tezos.WalletTransaction.CallContract(contractAddress, entrypoint, input);

#if UNITY_IOS || UNITY_ANDROID
			Application.OpenURL("tezos://");
#endif
		}

		public void MintFA2(Action<TokenBalance> callback)
		{
			var rnd = new Random();
			var randomInt = rnd.Next(1, int.MaxValue);
			var randomAmount = rnd.Next(1, 1024);

			var activeAccount = Tezos.WalletAccount.GetWalletAddress();
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
			CoroutineRunner.Instance.StartCoroutine(TezosManager.Instance.Tezos.GetCurrentWalletBalance(result =>
			{
				if (result.Success)
				{
					// Handle the successful retrieval of the wallet balance
					callback(result.Data);
				}
				else
				{
					// Handle the error case, update UI or log error
					Logger.LogError(result.ErrorMessage);
				}
			}));
		}

		public void GetSoftBalance(Action<int> callback)
		{
			GetSoftBalanceRoutine(callback);
		}

		public void TransferItem(int itemID, int amount, string address)
		{
			var sender = Tezos.WalletAccount.GetWalletAddress();

			Logger.LogDebug($"Transfering item {itemID} from {sender} to Address: {address}");

			const string entrypoint = "transfer";

			var input = "[ { \"prim\": \"Pair\", \"args\": [ { \"string\": \"" + sender +
			            "\" }, [ { \"prim\": \"Pair\", \"args\": [ { \"string\": \"" + address +
			            "\" }, { \"prim\": \"Pair\", \"args\": [ { \"int\": \"" + itemID + "\" }, { \"int\": \"" +
			            amount + "\" } ] } ] } ] ] } ]";

			Tezos.WalletTransaction.CallContract(contractAddress, entrypoint, input);

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

			Tezos.WalletTransaction.CallContract(contractAddress, entryPoint, parameter);

#if UNITY_IOS || UNITY_ANDROID
			Application.OpenURL("tezos://");
#endif
		}

		public void RemoveItemFromMarket(int itemID)
		{
			Debug.Log("Removing Item " + itemID + " from market.");

			const string entryPoint = "removeFromMarket";

			var sender = Tezos.WalletAccount.GetWalletAddress();

			var parameter = new MichelinePrim
			{
				Prim = PrimType.Pair,
				Args = new List<IMicheline>
				{
					new MichelineString(sender),
					new MichelineInt(itemID)
				}
			}.ToJson();

			Tezos.WalletTransaction.CallContract(contractAddress, entryPoint, parameter);

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
			PlayerPrefs.SetString("CurrentContract:" + Tezos.WalletAccount.GetWalletAddress(), activeContractAddress);
			Tezos.TokenContract.Address = activeContractAddress;
		}

		public void GetCoins()
		{
			const string entryPoint = "login";
			const string parameter = "{\"prim\": \"Unit\"}";

			Tezos.WalletTransaction.CallContract(contractAddress, entryPoint, parameter);

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
					var boolString = result.Data.GetProperty("prim");
					var boolVal = boolString.GetString() == "True";
					callback?.Invoke(boolVal);
				}));
		}

		public void RequestSignPayload(SignPayloadType signingType, string payload)
		{
			Tezos.WalletTransaction.RequestSignPayload(signingType, payload);

#if UNITY_IOS || UNITY_ANDROID
			Application.OpenURL("tezos://");
#endif
		}

		public bool VerifyPayload(SignPayloadType signingType, string payload)
		{
			return Tezos.WalletTransaction.VerifySignedPayload(signingType, payload);
		}

		public string GetActiveAccountAddress()
		{
			return Tezos.WalletAccount.GetWalletAddress();
		}

		public void Login(WalletProviderType walletProvider)
		{
			Tezos.WalletConnection.Connect(walletProvider);
		}

		public IWalletEventManager GetWalletMessageReceiver()
		{
			return Tezos.WalletEventProvider.EventManager;
		}

		public void GetOriginatedContracts(Action<IEnumerable<TokenContract>> callback)
		{
			GetOriginatedContracts(result =>
			{
				if (result.Success)
				{
					callback?.Invoke(result.Data);
				}
				else
				{
					Logger.LogError(result.ErrorMessage);
				}
			});
		}

		public void OnReady()
		{
		}

		private void GetOriginatedContracts(Action<HttpResult<IEnumerable<TokenContract>>> callback)
		{
			var routine = Tezos.GetOriginatedContracts(callback);
			CoroutineRunner.Instance.StartWrappedCoroutine(routine);
		}

		private void ProcessInventoryResult(JsonElement data, Action<List<IItemModel>> callback)
		{
			CoroutineRunner.Instance.StartWrappedCoroutine(
				NetezosExtensions.HumanizeValue<ContractInventoryViewResult[]>(data, _networkRPC, contractAddress,
					"humanizeInventory", inventoryResult => { OnInventoryFetched(inventoryResult, callback); }));
		}

		private string SerializeInput(object input)
		{
			return JsonSerializer.Serialize(input, JsonOptions.DefaultOptions);
		}

		private void OnInventoryFetched(ContractInventoryViewResult[] inventory, Action<List<IItemModel>> callback)
		{
			var dummyItemList = new List<IItemModel>();
			var owner = Tezos.WalletAccount.GetWalletAddress();

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
			var caller = Tezos.WalletAccount.GetWalletAddress();

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
					var intProp = result.Data.GetProperty("int");
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