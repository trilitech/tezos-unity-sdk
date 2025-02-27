using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beacon.Sdk.Beacon.Operation;
using Tezos.API;
using Tezos.MainThreadDispatcher;
using Tezos.WalletProvider;
using TezosSDK.Samples.MarketplaceSample.MarketplaceExample.Core;
using TezosSDK.Samples.MarketplaceSample.MarketplaceExample.UI;
using TMPro;
using UnityEngine;
using OperationResponse = Tezos.Operation.OperationResponse;

namespace TezosSDK.Samples.MarketplaceSample.MarketplaceExample
{

	public class UIManager : MonoBehaviour
	{
		[Header("References:")] [SerializeField]
		private GameObject loginPanel;

		[SerializeField] private GameObject welcomePanel;
		[SerializeField] private TextMeshProUGUI welcomeText;
		[SerializeField] private TabButton inventoryButton;
		[SerializeField] private TabButton marketButton;
		[SerializeField] private InventoryManager inventory;
		[SerializeField] private MarketManager market;
		[SerializeField] private GameObject loadingPanel;
		[SerializeField] private GameObject deployPanel;
		[SerializeField] private TextMeshProUGUI accountText;
		[SerializeField] private TextMeshProUGUI contractAddressText;
		[SerializeField] private TextMeshProUGUI balanceText;
		[SerializeField] private TextMeshProUGUI softBalanceText;
		[SerializeField] private GameObject popupPanel;
		[SerializeField] private GameObject initContractPanel;

		private IExampleManager _manager;

		private void Start()
		{
			_manager = ExampleFactory.Instance.GetExampleManager();
			InitializeCallbacks();

			AllowUIAccess(false);
			inventoryButton.OnTabSelected.AddListener(AccessInventory);
			marketButton.OnTabSelected.AddListener(AccessMarket);
			inventory.onInventoryRefresh.AddListener(AccessInventory);
			inventory.onItemMint.AddListener(MintItem);
		}

		private void OnOperationInjected(OperationResponse response)
		{
			if (!string.IsNullOrEmpty(response.TransactionHash))
			{
				_manager.FetchMarketItems(PopulateMarket);
				_manager.FetchInventoryItems(PopulateInventory);
				market.CheckSelection();
				DisplayWalletData();
			}
	
			DisplayPopup("Call injected!\n \n" + "\n \nTransaction Hash:\n" + response.TransactionHash);
		}

		private void OnWalletConnected(WalletProviderData walletProviderData)
		{
			if (!string.IsNullOrEmpty(walletProviderData.WalletAddress))
			{
				OnSignIn(true);
			}
		}

		private void OnWalletConnectionFailed(string errorInfo)
		{
			DisplayPopup("Wallet connection failed!\n \n" + "Response: \n" + errorInfo);
		}

		private void OnWalletDisconnected()
		{
			AllowUIAccess(false);
			ResetWalletData();
		}

		public void AllowUIAccess(bool allow)
		{
			loginPanel.SetActive(!allow);
			welcomePanel.SetActive(allow);
			inventoryButton.gameObject.SetActive(allow);
			marketButton.gameObject.SetActive(allow);
		}

		public void ChangeContract()
		{
			_manager.GetOriginatedContracts(contracts =>
			{
				if (contracts != null && contracts.Any())
				{
					if (string.IsNullOrEmpty(_manager.Tezos.TokenContract.Address))
					{
						return;
					}

					initContractPanel.SetActive(true);
				}
				else
				{
					DisplayPopup("You do not have deployed contracts yet!");
				}
			});
		}

		public void DisplayPopup(string message)
		{
			UnityMainThreadDispatcher.Instance().Enqueue(() =>
			{
				popupPanel.SetActive(true);
				popupPanel.GetComponentInChildren<TMP_InputField>().text = message;
			});
		}

		public void GetCoins()
		{
			_manager.GetCoins();
		}

		public void InitContract(string address)
		{
			// todo: switch inventory items
			_manager.ChangeContract(address);
			UpdateContractAddress();
		}

		public void OnSignIn(bool success)
		{
			_manager.GetOriginatedContracts(contracts =>
			{
				if (contracts != null && contracts.Any())
				{
					if (string.IsNullOrEmpty(_manager.Tezos.TokenContract.Address))
					{
						initContractPanel.SetActive(true);
					}

					initContractPanel.GetComponent<InitiateContractController>()
						.DrawContractToggles(contracts, _manager.Tezos.WalletAccount.GetWalletAddress());
				}

				AllowUIAccess(success);
			});

			//TODO: GetActiveAccount() in the BeaconConnector SHOULD be returning stuff from the paired account.
			//Something in there might be usable to populate the User info I removed if we still want this.
			welcomeText.text = success ? "Welcome!" : "Pairing failed or timed out";
			DisplayWalletData();
		}

		public void ResetWalletData()
		{
			SetAccountText(string.Empty);
			SetBalanceText(0);
			SetSoftBalanceText(0);
			SetContract(string.Empty);
		}

		public void ShowDeployPanel()
		{
			deployPanel.SetActive(true);
		}

		public void UpdateContractAddress()
		{
			var currentContractAddressText = _manager.Tezos.TokenContract.Address;

			if (contractAddressText.text != currentContractAddressText)
			{
				SetContract(currentContractAddressText);
			}
		}

		public void UpdateContracts()
		{
			_manager.GetOriginatedContracts(contracts =>
			{
				if (contracts == null || contracts.Count() <= 0)
				{
					return;
				}

				initContractPanel.GetComponent<InitiateContractController>()
					.DrawContractToggles(contracts, _manager.Tezos.WalletAccount.GetWalletAddress());
			});
		}

		private void AccessInventory()
		{
			loadingPanel.SetActive(true);

			_manager.FetchInventoryItems(PopulateInventory);

			DisplayWalletData();
		}

		private void AccessMarket()
		{
			loadingPanel.SetActive(true);

			_manager.FetchMarketItems(PopulateMarket);

			DisplayWalletData();
		}

		private void DisplayWalletData()
		{
			var address = _manager.GetActiveAccountAddress();
			SetAccountText(address);
			UpdateContractAddress();
			_manager.GetBalance(SetBalanceText);
			_manager.GetSoftBalance(SetSoftBalanceText);
		}

		private IEnumerator DoActionNextFrame(Action action)
		{
			yield return new WaitForEndOfFrame();
			action?.Invoke();
		}

		private void InitializeCallbacks()
		{
			
			TezosAPI.WalletConnected += OnWalletConnected;
			TezosAPI.WalletDisconnected += OnWalletDisconnected;
			TezosAPI.OperationResulted += OnOperationInjected;
			
			_manager.GetWalletMessageReceiver().WalletConnected += OnWalletConnected;
			_manager.GetWalletMessageReceiver().WalletConnectionFailed += OnWalletConnectionFailed;
			_manager.GetWalletMessageReceiver().WalletDisconnected += OnWalletDisconnected;
		}

		private void MintItem()
		{
			_manager.MintItem();
		}

		private void PopulateInventory(List<IItemModel> items)
		{
			Action action = () =>
			{
				inventory.Init(items);
				loadingPanel.SetActive(false);
			};

			StartCoroutine(DoActionNextFrame(action));
		}

		private void PopulateMarket(List<IItemModel> items)
		{
			Action action = () =>
			{
				market.Init(items);
				loadingPanel.SetActive(false);
			};

			StartCoroutine(DoActionNextFrame(action));
		}

		private void SetAccountText(string account)
		{
			accountText.text = account;
		}

		private void SetBalanceText(ulong balance)
		{
			// balance is in mutez (one millionth of tezos)
			var floatBalance = balance * 0.000001;
			balanceText.text = floatBalance.ToString();
		}

		private void SetContract(string contractAddress)
		{
			contractAddressText.text = contractAddress;
		}

		private void SetSoftBalanceText(int balance)
		{
			// balance is in mutez (one millionth of tezos)
			softBalanceText.text = balance.ToString();
		}
	}

}