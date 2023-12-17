using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;
using UnityEngine;

namespace TezosSDK.View
{
    public class TezosAuthenticator : MonoBehaviour
    {
        private ITezos Tezos { get; set; }

        [SerializeField] private QRCodeView qrCodeView;
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private GameObject deepLinkButton;
        [SerializeField] private GameObject socialLoginButton;
        [SerializeField] private GameObject logoutButton;
        [SerializeField] private GameObject qrCodePanel;

        // Platform flags to determine the current running platform
        private bool _isMobile;
        private bool _isWebGL;

        private void Start()
        {
            InitializeTezos();
            SetPlatformFlags();
        }

        private void InitializeTezos()
        {
            Tezos = TezosManager.Instance.Tezos;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // Subscribe to wallet events for handling user authentication.
            Tezos.Wallet.EventManager.HandshakeReceived += OnHandshakeReceived;
            Tezos.Wallet.EventManager.AccountConnected += OnAccountConnected;
            Tezos.Wallet.EventManager.AccountDisconnected += OnAccountDisconnected;
        }

        private void SetPlatformFlags()
        {
            _isMobile = Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android;
            _isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;

            if (_isWebGL)
            {
                ToggleUIElements(isAuthenticated: false);
                Tezos.Wallet.OnReady();
            }
        }

        private void OnHandshakeReceived(HandshakeData handshakeData)
        {
            ToggleUIElements(isAuthenticated: false);
            qrCodeView.SetQrCode(handshakeData);
        }

        private void OnAccountConnected(AccountInfo accountInfo)
        {
            ToggleUIElements(isAuthenticated: true);
        }

        private void OnAccountDisconnected(AccountInfo accountInfo)
        {
        }

        public void DisconnectWallet()
        {
            ToggleUIElements(isAuthenticated: false);
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

        /// <summary>
        /// Toggles the UI elements based on authentication status.
        /// </summary>
        /// <param name="isAuthenticated">Indicates whether the user is authenticated.</param>
        private void ToggleUIElements(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                deepLinkButton.SetActive(false);
                socialLoginButton.SetActive(false);
                qrCodePanel.SetActive(false);
            }
            else
            {
                // Activate deepLinkButton when on mobile or WebGL, but not authenticated
                deepLinkButton.SetActive(_isMobile || _isWebGL);

                // Activate socialLoginButton only when on WebGL and not authenticated
                socialLoginButton.SetActive(_isWebGL);

                // Activate qrCodePanel only on standalone and not authenticated
                qrCodePanel.SetActive(!_isMobile && !_isWebGL);
            }

            logoutButton.SetActive(isAuthenticated);

            if (contentPanel != null)
            {
                contentPanel.SetActive(isAuthenticated);
            }
        }

        private void OnDisable()
        {
            if (Tezos == null) return;
            
            Tezos.Wallet.EventManager.HandshakeReceived -= OnHandshakeReceived;
            Tezos.Wallet.EventManager.AccountConnected -= OnAccountConnected;
            Tezos.Wallet.EventManager.AccountDisconnected -= OnAccountDisconnected;
        }
    }
    
}