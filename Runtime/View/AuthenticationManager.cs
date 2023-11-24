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
        [SerializeField] private GameObject logoutButton;
        [SerializeField] private GameObject qrCodePanel;

        private bool _isMobile;

        void Start()
        {
#if (UNITY_IOS || UNITY_ANDROID)
		_isMobile = true;
#else
            _isMobile = false;
#endif
            Tezos = TezosManager.Instance.Tezos;
            Tezos.Wallet.EventManager.HandshakeReceived += OnHandshakeReceived;
            Tezos.Wallet.EventManager.AccountConnected += OnAccountConnected;
            Tezos.Wallet.EventManager.AccountDisconnected += OnAccountDisconnected;
        }

        void OnHandshakeReceived(HandshakeData handshakeData)
        {
            EnableUI(isAuthenticated: false);
            qrCodeView.SetQrCode(handshakeData);
        }

        void OnAccountConnected(AccountInfo account_info)
        {
            EnableUI(isAuthenticated: true);
            Debug.Log("OnAccountConnected");
        }

        void OnAccountDisconnected(AccountInfo account_info)
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

        void EnableUI(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                deepLinkButton.SetActive(false);
                qrCodePanel.SetActive(false);
            }
            else
            {
                if (_isMobile)
                {
                    deepLinkButton.SetActive(true);
                    qrCodePanel.SetActive(false);
                }
                else
                {
                    qrCodePanel.SetActive(true);
                    deepLinkButton.SetActive(false);
                }
            }

            logoutButton.SetActive(isAuthenticated);

            if (contentPanel == null) return;
            contentPanel.SetActive(isAuthenticated);
        }
    }
}