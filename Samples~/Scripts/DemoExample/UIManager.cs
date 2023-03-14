using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[Header("References:")]
	[SerializeField] private GameObject loginPanel;
	[SerializeField] private GameObject welcomePanel;
	[SerializeField] private TMPro.TextMeshProUGUI welcomeText;
	[SerializeField] private TabButton inventoryButton;
	[SerializeField] private TabButton marketButton;
	[SerializeField] private InventoryManager inventory;
	[SerializeField] private MarketManager market;
	[SerializeField] private GameObject loadingPanel;
	[SerializeField] private TMPro.TextMeshProUGUI accountText;
	[SerializeField] private TMPro.TextMeshProUGUI balanceText;
	[SerializeField] private TMPro.TextMeshProUGUI softBalanceText;
	[SerializeField] private GameObject popupPanel;

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

	private void InitializeCallbacks()
	{
		_manager.GetMessageReceiver().AccountConnected += OnAccountConnected;
		_manager.GetMessageReceiver().AccountConnectionFailed += OnAccountConnectionFailed;
		_manager.GetMessageReceiver().AccountDisconnected += OnAccountDisconnected;
		_manager.GetMessageReceiver().ContractCallCompleted += OnContractCallCompleted;
		_manager.GetMessageReceiver().ContractCallFailed += OnContractCallFailed;
		_manager.GetMessageReceiver().ContractCallInjected += OnContractCallInjected;
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

	private void MintItem()
	{
		_manager.MintItem();
	}

	public void GetCoins()
	{
		_manager.GetCoins();
	}

	public void OnSignIn(bool success)
	{
		AllowUIAccess(success);
		//TODO: GetActiveAccount() in the BeaconConnector SHOULD be returning stuff from the paired account.
		//Something in there might be usable to populate the User info I removed if we still want this.
		welcomeText.text = success? "Welcome!" : "Pairing failed or timed out";
		DisplayWalletData();
	}

	public void AllowUIAccess(bool allow)
	{
		loginPanel.SetActive(!allow);
		welcomePanel.SetActive(allow);
		inventoryButton.gameObject.SetActive(allow);
		marketButton.gameObject.SetActive(allow);
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

	private IEnumerator DoActionNextFrame(Action action)
	{
		yield return new WaitForEndOfFrame();
		action?.Invoke();
	}
	
	public void ResetWalletData()
	{
		SetAccountText("0");
		SetBalanceText(0);
		SetSoftBalanceText(0);
	}

	private void DisplayWalletData()
	{
		string address = _manager.GetActiveAccountAddress();
		SetAccountText(address);
		_manager.GetBalance(SetBalanceText);
		_manager.GetSoftBalance(SetSoftBalanceText);
	}

	private void SetBalanceText(ulong balance)
	{
		// balance is in mutez (one millionth of tezos)
		var floatBalance = balance * 0.000001;
		balanceText.text = (floatBalance).ToString();
	}
	
	private void SetSoftBalanceText(int balance)
	{
		// balance is in mutez (one millionth of tezos)
		softBalanceText.text = balance.ToString();
	}

	private void SetAccountText(string account)
	{
		accountText.text = account;
	}

	private void DisplayPopup(string message)
	{
		popupPanel.SetActive(true);
		popupPanel.GetComponentInChildren<TMPro.TMP_InputField>().text = message;
	}
	
#region Tezos Callbacks

	private void OnAccountConnected(string account)
	{
		if (!string.IsNullOrEmpty(account))
			OnSignIn(true);
	}
	
	private void OnAccountConnectionFailed(string response)
	{
		DisplayPopup("Wallet connection failed!\n \n" +
		             "Response: \n" + response);
	}

	private void OnAccountDisconnected(string account)
	{
		AllowUIAccess(false);
		ResetWalletData();
	}

	[Serializable]
	private struct Transaction
	{
		public string transactionHash;
	}

	private void OnContractCallCompleted(string response)
	{
		string transactionHash =
			JsonSerializer.Deserialize<JsonElement>(response).GetProperty("transactionHash").ToString();
		DisplayPopup("Transaction completed!\n \n" +
		             "Transaction hash:\n" + transactionHash +
		             "\n \nResponse:\n" + response);
	}
	
	private void OnContractCallFailed(string response)
	{
		DisplayPopup("Transaction failed!\n \n" +
		             "Response:\n" + response);
	}
	
	private void OnContractCallInjected(string result)
	{
		string successString = JsonSerializer.Deserialize<JsonElement>(result).GetProperty("success").ToString();
		string transactionHash = JsonSerializer.Deserialize<JsonElement>(result).GetProperty("transactionHash").ToString();
		bool success = successString != null && bool.Parse(successString);
		if (success)
		{
			_manager.FetchMarketItems(PopulateMarket);
			_manager.FetchInventoryItems(PopulateInventory);
			market.CheckSelection();
			DisplayWalletData();
		}

		DisplayPopup("Call injected!\n \n" +
		             "Response:\n" + success +
		             "\n \nTransaction Hash:\n" + transactionHash);
	}
#endregion
}
