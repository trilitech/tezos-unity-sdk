#region

using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;
using UnityEngine;

#endregion

namespace TezosSDK.View
{

	public class TezosAuthenticator : MonoBehaviour
	{
		[SerializeField] private QRCodeView qrCodeView;
		[SerializeField] private GameObject contentPanel;
		[SerializeField] private GameObject deepLinkButton;
		[SerializeField] private GameObject socialLoginButton;
		[SerializeField] private GameObject logoutButton;
		[SerializeField] private GameObject qrCodePanel;

		// Platform flags to determine the current running platform
		private bool _isMobile;
		private bool _isWebGL;

		private ITezos Tezos { get; set; }

		private void Start()
		{
			InitializeTezos();
			SetPlatformFlags();
		}

		private void OnDisable()
		{
			if (Tezos == null)
			{
				return;
			}

			Tezos.Wallet.EventManager.HandshakeReceived -= OnHandshakeReceived;
			Tezos.Wallet.EventManager.AccountConnected -= OnAccountConnected;
			Tezos.Wallet.EventManager.AccountDisconnected -= OnAccountDisconnected;
		}

		private void OnAccountConnected(AccountInfo accountInfo)
		{
			ToggleUIElements(true);
		}

		private void OnAccountDisconnected(AccountInfo accountInfo)
		{
		}

		private void OnHandshakeReceived(HandshakeData handshakeData)
		{
			ToggleUIElements(false);
			qrCodeView.SetQrCode(handshakeData);
		}

		public void ConnectByDeeplink()
		{
			Tezos.Wallet.Connect(WalletProviderType.beacon);
		}

		public void ConnectWithSocial()
		{
			Tezos.Wallet.Connect(WalletProviderType.kukai);
		}

		public void DisconnectWallet()
		{
			ToggleUIElements(false);
			Tezos.Wallet.Disconnect();
		}

		private void InitializeTezos()
		{
			Tezos = TezosManager.Instance.Tezos;
			SubscribeToEvents();
		}

		private void SetPlatformFlags()
		{
			_isMobile = Application.platform == RuntimePlatform.IPhonePlayer ||
			            Application.platform == RuntimePlatform.Android;

			_isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;

			if (_isWebGL)
			{
				ToggleUIElements(false);
				Tezos.Wallet.OnReady();
			}
		}

		private void SubscribeToEvents()
		{
			// Subscribe to wallet events for handling user authentication.
			Tezos.Wallet.EventManager.HandshakeReceived += OnHandshakeReceived;
			Tezos.Wallet.EventManager.AccountConnected += OnAccountConnected;
			Tezos.Wallet.EventManager.AccountDisconnected += OnAccountDisconnected;
		}

        /// <summary>
        ///     Toggles the UI elements based on authentication status.
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
	}

}