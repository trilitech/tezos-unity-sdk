using TezosSDK.API;
using TezosSDK.WalletProvider;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefactorExample : MonoBehaviour
{
        [SerializeField] private TMP_Text _informationText;
        [SerializeField] private Button   _connectButton;
        [SerializeField] private Button   _disconnectButton;
        [SerializeField] private Button   _logInButton;
        [SerializeField] private Button   _logOutButton;
        [SerializeField] private Button   _fetchBalanceButton;
            
        private void Awake()
        {
            _connectButton.onClick.AddListener(OnConnectClicked);
            _disconnectButton.onClick.AddListener(OnDisconnectClicked);
            _logInButton.onClick.AddListener(OnLogInClicked);
            _logOutButton.onClick.AddListener(OnLogOutClicked);
            _fetchBalanceButton.onClick.AddListener(OnFetchBalanceClicked);
        }

        private async void OnFetchBalanceClicked()
        {
            int convertedBalance = (int)(await TezosAPI.GetXTZBalance() / 1000000);
            _informationText.text = $"Balance: {convertedBalance}";
        }

        private async void OnConnectClicked()
        {
            var result = await TezosAPI.ConnectWallet(new WalletProviderData { WalletType = WalletType.BEACON });
            _informationText.text = $"{result.WalletAddress}";
        }

        private async void OnDisconnectClicked()
        {
            var result = await TezosAPI.DisconnectWallet();
            _informationText.text = $"{result}";
        }
            
        private async void OnLogInClicked()
        {
            var result = await TezosAPI.SocialLogIn(new SocialProviderData { SocialLoginType = SocialLoginType.Kukai });
            _informationText.text = $"{result.WalletAddress}";
        }

        private async void OnLogOutClicked()
        {
            var result = await TezosAPI.SocialLogOut();
            _informationText.text = $"{result}";
        }
}
