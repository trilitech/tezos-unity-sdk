using TezosSDK.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos
{

	public class TezosAuthenticator : MonoBehaviour
	{
		[SerializeField] private QrCodeGenerator qrCodeGenerator;
		[SerializeField] private GameObject deepLinkButton;
		[SerializeField] private GameObject socialLoginButton;
		[SerializeField] private GameObject logoutButton;
		[SerializeField] private Image darkBG;

		// Platform flags to determine the current running platform
		private bool _isMobile;
		private bool _isWebGL;

		private ITezos Tezos { get; set; }

		private void Start()
		{
			Tezos = TezosManager.Instance.Tezos;
			SubscribeToEvents();
			SetPlatformFlags();

			if (Tezos.Wallet.IsConnected)
			{
				Logger.LogDebug("TezosAuthenticator.Start: Wallet is connected");
				ToggleUIElements(true);
			}
			else
			{
				Logger.LogDebug("TezosAuthenticator.Start: Wallet is not connected");
				ToggleUIElements(false);

				// Call Connect() only when on standalone
				if (!_isWebGL && !_isMobile)
				{
					Tezos.Wallet.Connect(WalletProviderType.beacon);
				}
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				Tezos.Wallet.Connect(WalletProviderType.beacon);
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Tezos.Wallet.Disconnect();
			}
		}

		private void OnEnable()
		{
			Logger.LogDebug("TezosAuthenticator.OnEnable");

			if (TezosManager.Instance != null && !TezosManager.Instance.Wallet.IsConnected &&
			    TezosManager.Instance.Wallet.HandshakeData != null)
			{
				qrCodeGenerator.SetQrCode(TezosManager.Instance.Wallet.HandshakeData);
			}
		}

		private void OnDisable()
		{
			Logger.LogDebug("TezosAuthenticator.OnDisable");
			UnsubscribeFromEvents();
		}

		private void OnDestroy()
		{
			Logger.LogDebug("TezosAuthenticator.OnDestroy");
			UnsubscribeFromEvents();
		}

		private void OnHandshakeReceived(HandshakeData handshakeData)
		{
			Logger.LogDebug("TezosAuthenticator.OnHandshakeReceived");
			qrCodeGenerator.SetQrCode(handshakeData);
		}

		private void OnWalletConnected(WalletInfo walletInfo)
		{
			Logger.LogDebug("TezosAuthenticator.OnWalletConnected");
			ToggleUIElements(true);
		}

		public void ConnectByDeeplink()
		{
			Logger.LogDebug("TezosAuthenticator.ConnectByDeeplink");
			Tezos.Wallet.Connect(WalletProviderType.beacon);
		}

		public void ConnectWithSocial()
		{
			Logger.LogDebug("TezosAuthenticator.ConnectWithSocial");
			Tezos.Wallet.Connect(WalletProviderType.kukai);
		}

		public void DisconnectWallet()
		{
			Tezos.Wallet.Disconnect();
		}

		private void SetPlatformFlags()
		{
			_isMobile = Application.platform == RuntimePlatform.IPhonePlayer ||
			            Application.platform == RuntimePlatform.Android;

			_isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
		}

		private void SubscribeToEvents()
		{
			// Subscribe to wallet events for handling user authentication.
			Tezos.Wallet.EventManager.HandshakeReceived += OnHandshakeReceived;
			Tezos.Wallet.EventManager.WalletConnected += OnWalletConnected;
		}

		/// <summary>
		///     Toggles the UI elements based on authentication status.
		/// </summary>
		/// <param name="isAuthenticated">Indicates whether the user is authenticated.</param>
		private void ToggleUIElements(bool isAuthenticated)
		{
			Logger.LogDebug($"ToggleUIElements: isAuthenticated: {isAuthenticated}");

			if (isAuthenticated)
			{
				deepLinkButton.SetActive(false);
				socialLoginButton.SetActive(false);
				qrCodeGenerator.gameObject.SetActive(false);
				darkBG.gameObject.SetActive(false);
			}
			else
			{
				// Activate deepLinkButton when on mobile or WebGL, but not authenticated
				deepLinkButton.SetActive(_isMobile || _isWebGL);

				// Activate socialLoginButton only when on WebGL and not authenticated
				socialLoginButton.SetActive(_isWebGL);

				// Activate qrCodePanel only on standalone and not authenticated
				qrCodeGenerator.gameObject.SetActive(!_isMobile && !_isWebGL);

				darkBG.gameObject.SetActive(true);
			}

			logoutButton.SetActive(isAuthenticated);
		}

		private void UnsubscribeFromEvents()
		{
			Logger.LogDebug("TezosAuthenticator.UnsubscribeFromEvents");
			TezosManager.Instance.Wallet.EventManager.HandshakeReceived -= OnHandshakeReceived;
			TezosManager.Instance.Wallet.EventManager.WalletConnected -= OnWalletConnected;
		}
	}

}