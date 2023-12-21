#region

using System;
using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#endregion

namespace TezosSDK.View
{

	public class TezosAuthenticator : MonoBehaviour
	{
		[SerializeField] private QRCodeView qrCodeView;
		[SerializeField] private GameObject deepLinkButton;
		[SerializeField] private GameObject socialLoginButton;
		[SerializeField] private GameObject logoutButton;
		[SerializeField] private Image darkBG;
		

		// Platform flags to determine the current running platform
		private bool _isMobile;
		private bool _isWebGL;

		private ITezos Tezos { get; set; }

		private void Awake()
		{
			ToggleUIElements(false);
		}

		private void Start()
		{
			InitializeTezos();
			SetPlatformFlags();
			
			if (TezosManager.Instance.Tezos.Wallet.IsConnected)
			{
				ToggleUIElements(true);
			}
		}

		private void OnDisable()
		{
			if (Tezos == null)
			{
				return;
			}

			Tezos.Wallet.EventManager.HandshakeReceived -= OnHandshakeReceived;
			Tezos.Wallet.EventManager.WalletConnected -= OnWalletConnected;
			Tezos.Wallet.EventManager.WalletDisconnected -= OnWalletDisconnected;
		}

		private void OnEnable()
		{
			if (TezosManager.Instance != null && !TezosManager.Instance.Wallet.IsConnected &&
			    TezosManager.Instance.Wallet.HandshakeData != null)
			{
				qrCodeView.SetQrCode(TezosManager.Instance.Wallet.HandshakeData);
			}
		}

		private void OnWalletConnected(WalletInfo walletInfo)
		{
			ToggleUIElements(true);
		}

		private void OnWalletDisconnected(WalletInfo walletInfo)
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
			Tezos.Wallet.EventManager.WalletConnected += OnWalletConnected;
			Tezos.Wallet.EventManager.WalletDisconnected += OnWalletDisconnected;
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
				qrCodeView.gameObject.SetActive(false);
				darkBG.gameObject.SetActive(false);
			}
			else
			{
				// Activate deepLinkButton when on mobile or WebGL, but not authenticated
				deepLinkButton.SetActive(_isMobile || _isWebGL);

				// Activate socialLoginButton only when on WebGL and not authenticated
				socialLoginButton.SetActive(_isWebGL);

				// Activate qrCodePanel only on standalone and not authenticated
				qrCodeView.gameObject.SetActive(!_isMobile && !_isWebGL);
				
				darkBG.gameObject.SetActive(true);
			}

			logoutButton.SetActive(isAuthenticated);
		}
	}

}