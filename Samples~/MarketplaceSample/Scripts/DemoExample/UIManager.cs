using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TezosSDK.Beacon;
using TezosSDK.Helpers;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Tezos;
using UnityEngine;

namespace TezosSDK.Samples.DemoExample
{
    public class UIManager : MonoBehaviour
    {
        [Header("References:")] [SerializeField]
        private GameObject loginPanel;

        [SerializeField] private GameObject welcomePanel;
        [SerializeField] private TMPro.TextMeshProUGUI welcomeText;
        [SerializeField] private TabButton inventoryButton;
        [SerializeField] private TabButton marketButton;
        [SerializeField] private InventoryManager inventory;
        [SerializeField] private MarketManager market;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject deployPanel;
        [SerializeField] private TMPro.TextMeshProUGUI accountText;
        [SerializeField] private TMPro.TextMeshProUGUI contractAddressText;
        [SerializeField] private TMPro.TextMeshProUGUI balanceText;
        [SerializeField] private TMPro.TextMeshProUGUI softBalanceText;
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

        private void InitializeCallbacks()
        {
            _manager.GetWalletMessageReceiver().WalletConnected += OnWalletConnected;
            _manager.GetWalletMessageReceiver().WalletConnectionFailed += OnWalletConnectionFailed;
            _manager.GetWalletMessageReceiver().WalletDisconnected += OnWalletDisconnected;
            _manager.GetWalletMessageReceiver().ContractCallCompleted += OnContractCallCompleted;
            _manager.GetWalletMessageReceiver().ContractCallFailed += OnContractCallFailed;
            _manager.GetWalletMessageReceiver().ContractCallInjected += OnContractCallInjected;
            _manager.GetWalletMessageReceiver().PayloadSigned += OnPayloadSigned;
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
            _manager.GetOriginatedContracts(contracts =>
            {
                if (contracts != null && contracts.Any())
                {
                    if (string.IsNullOrEmpty(_manager.Tezos.TokenContract.Address))
                        initContractPanel.SetActive(true);

                    initContractPanel.GetComponent<InitiateContractController>()
                        .DrawContractToggles(contracts, _manager.Tezos.Wallet.GetWalletAddress());
                }

                AllowUIAccess(success);
            });

            //TODO: GetActiveAccount() in the BeaconConnector SHOULD be returning stuff from the paired account.
            //Something in there might be usable to populate the User info I removed if we still want this.
            welcomeText.text = success ? "Welcome!" : "Pairing failed or timed out";
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
            CoroutineRunner.Instance.StartWrappedCoroutine(DoActionNextFrame(action));
        }

        private void PopulateMarket(List<IItemModel> items)
        {
            Action action = () =>
            {
                market.Init(items);
                loadingPanel.SetActive(false);
            };
            CoroutineRunner.Instance.StartWrappedCoroutine(DoActionNextFrame(action));
        }

        private IEnumerator DoActionNextFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        public void ResetWalletData()
        {
            SetAccountText(string.Empty);
            SetBalanceText(0);
            SetSoftBalanceText(0);
            SetContract(string.Empty);
        }

        private void DisplayWalletData()
        {
            var address = _manager.GetActiveAccountAddress();
            SetAccountText(address);
            UpdateContractAddress();
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

        private void SetContract(string contractAddress)
        {
            contractAddressText.text = contractAddress;
        }

        public void UpdateContracts()
        {
            _manager.GetOriginatedContracts(contracts =>
            {
                if (contracts == null || contracts.Count() <= 0) return;

                initContractPanel.GetComponent<InitiateContractController>()
                    .DrawContractToggles(contracts, _manager.Tezos.Wallet.GetWalletAddress());
            });
        }

        public void DisplayPopup(string message)
        {
            UnityMainThreadDispatcher.Enqueue((msg) =>
            {
                popupPanel.SetActive(true);
                popupPanel.GetComponentInChildren<TMPro.TMP_InputField>().text = msg;
            }, message);
        }

        public void ShowDeployPanel()
        {
            deployPanel.SetActive(true);
        }

        #region Tezos Callbacks

        private void OnWalletConnected(WalletInfo walletInfo)
        {
            if (!string.IsNullOrEmpty(walletInfo.Address))
                OnSignIn(true);
        }

        private void OnWalletConnectionFailed(ErrorInfo errorInfo)
        {
            DisplayPopup("Wallet connection failed!\n \n" +
                         "Response: \n" + errorInfo.Message);
        }

        private void OnWalletDisconnected(WalletInfo walletInfo)
        {
            AllowUIAccess(false);
            ResetWalletData();
        }

        private void OnContractCallCompleted(OperationResult operationResult)
        {
            DisplayPopup($"Transaction completed with hash {operationResult.TransactionHash}");
        }

        public void UpdateContractAddress()
        {
            var currentContractAddressText = _manager
                .Tezos
                .TokenContract
                .Address;

            if (contractAddressText.text != currentContractAddressText)
            {
                SetContract(currentContractAddressText);
            }
        }

        public void InitContract(string address)
        {
            // todo: switch inventory items
            _manager.ChangeContract(address);
            UpdateContractAddress();
        }

        public void ChangeContract()
        {
            _manager.GetOriginatedContracts(contracts =>
            {
                if (contracts != null && contracts.Any())
                {
                    if (string.IsNullOrEmpty(_manager.Tezos.TokenContract.Address)) return;

                    initContractPanel.SetActive(true);
                }
                else
                {
                    DisplayPopup("You do not have deployed contracts yet!");
                }
            });
        }

        private void OnContractCallFailed(ErrorInfo errorInfo)
        {
            DisplayPopup($"Transaction failed, error: {errorInfo.Message}");
        }

        private void OnContractCallInjected(OperationResult operationResult)
        {
            if (!string.IsNullOrEmpty(operationResult.TransactionHash))
            {
                _manager.FetchMarketItems(PopulateMarket);
                _manager.FetchInventoryItems(PopulateInventory);
                market.CheckSelection();
                DisplayWalletData();
            }

            DisplayPopup("Call injected!\n \n" +
                         "\n \nTransaction Hash:\n" + operationResult.TransactionHash);
        }

        private void OnPayloadSigned(SignResult signResult)
        {
            DisplayPopup($"Successfully signed with signature: {signResult.Signature}");
        }

        #endregion
    }
}