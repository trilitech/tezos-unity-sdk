using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;
using UnityEngine;

namespace TezosSDK.View
{
    public class AuthenticationManager : MonoBehaviour
    {
        private ITezos Tezos { get; set; }

        [SerializeField] private QRCodeView qrCodeView;
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private GameObject deepLinkButton;
        [SerializeField] private GameObject socialLoginButton;
        [SerializeField] private GameObject logoutButton;
        [SerializeField] private GameObject qrCodePanel;

        private bool _isMobile;
        private bool _isWebGL;

        void Start()
        {
            Tezos = TezosManager.Instance.Tezos;
            Tezos.Wallet.EventManager.HandshakeReceived += OnHandshakeReceived;
            Tezos.Wallet.EventManager.AccountConnected += OnAccountConnected;
            Tezos.Wallet.EventManager.AccountDisconnected += OnAccountDisconnected;

#if UNITY_STANDALONE || UNITY_EDITOR
            _isMobile = false;
            _isWebGL = false;
#elif (UNITY_IOS || UNITY_ANDROID)
		    _isMobile = true;
            _isWebGL = false;
#elif UNITY_WEBGL
		    _isMobile = false;
            _isWebGL = true;
            EnableUI(isAuthenticated: false);
            Tezos.Wallet.OnReady();
#endif
        }

        void OnHandshakeReceived(HandshakeData handshakeData)
        {
            EnableUI(isAuthenticated: false);
            qrCodeView.SetQrCode(handshakeData);
        }

        void OnAccountConnected(AccountInfo accountInfo)
        {
            EnableUI(isAuthenticated: true);
            Debug.Log("OnAccountConnected");
        }

        void OnAccountDisconnected(AccountInfo accountInfo)
        {
            Debug.Log("OnAccountDisconnected");
        }

        public void DisconnectWallet()
        {
            EnableUI(isAuthenticated: false);
            Tezos.Wallet.Disconnect();
        }

        public void ConnectByDeeplink()
        {
            Tezos.Wallet.Connect(WalletProviderType.beacon);
        }

        public void ConnectWithSocial()
        {
            Tezos.Wallet.Connect(WalletProviderType.kukai);
        }

        void EnableUI(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                deepLinkButton.SetActive(false);
                socialLoginButton.SetActive(false);
                qrCodePanel.SetActive(false);
            }
            else
            {
                if (_isMobile)
                {
                    deepLinkButton.SetActive(true);
                    socialLoginButton.SetActive(false);
                    qrCodePanel.SetActive(false);
                }
                else if (_isWebGL)
                {
                    deepLinkButton.SetActive(true);
                    socialLoginButton.SetActive(true);
                    qrCodePanel.SetActive(false);
                }
                else
                {
                    deepLinkButton.SetActive(false);
                    socialLoginButton.SetActive(false);
                    qrCodePanel.SetActive(true);
                }
            }

            logoutButton.SetActive(isAuthenticated);

            if (contentPanel == null) return;
            contentPanel.SetActive(isAuthenticated);
        }
    }
}