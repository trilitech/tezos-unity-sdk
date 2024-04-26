using TezosSDK.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.Tezos
{

	public class TezosAuthenticator : MonoBehaviour
	{
		[SerializeField] private QrCodeGenerator qrCodeGenerator;
		[SerializeField] private GameObject deepLinkButton;
		[SerializeField] private GameObject socialLoginButton;
		[SerializeField] private GameObject logoutButton;

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
				ToggleUIElements(true);
			}
			else
			{
				ToggleUIElements(false);

				// Call Connect() only when on standalone
				if (!_isWebGL && !_isMobile)
				{
					Tezos.Wallet.Connect(WalletProviderType.beacon);
				}
			}
		}

		// private void Update()
		// {
		// 	if (Input.GetKeyDown(KeyCode.Return))
		// 	{
		// 		Tezos.Wallet.Connect(WalletProviderType.beacon);
		// 	}
		//
		// 	if (Input.GetKeyDown(KeyCode.Escape))
		// 	{
		// 		Tezos.Wallet.Disconnect();
		// 	}
		// }

		private void OnEnable()
		{
			if (TezosManager.Instance != null && !TezosManager.Instance.Wallet.IsConnected &&
			    TezosManager.Instance.Wallet.HandshakeData != null)
			{
				qrCodeGenerator.SetQrCode(TezosManager.Instance.Wallet.HandshakeData);
			}
		}

		private void OnDisable()
		{
			UnsubscribeFromEvents();
		}

		private void OnDestroy()
		{
			UnsubscribeFromEvents();
		}

		private void OnHandshakeReceived(HandshakeData handshakeData)
		{
			qrCodeGenerator.SetQrCode(handshakeData);
		}

		private void OnWalletConnected(WalletInfo walletInfo)
		{
			ToggleUIElements(true);
		}

		private void OnWalletDisconnected(WalletInfo walletInfo)
		{
			Logger.LogDebug("TezosAuthenticator.OnWalletDisconnected");
			ToggleUIElements(false);
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
				qrCodeGenerator.gameObject.SetActive(false);
			}
			else
			{
				// Activate deepLinkButton when on mobile or WebGL, but not authenticated
				deepLinkButton.SetActive(_isMobile || _isWebGL);

				// Activate socialLoginButton only when on WebGL and not authenticated
				socialLoginButton.SetActive(_isWebGL);

				// Activate qrCodePanel only on standalone and not authenticated
				qrCodeGenerator.gameObject.SetActive(!_isMobile && !_isWebGL);
			}

			logoutButton.SetActive(isAuthenticated);
		}

		private void UnsubscribeFromEvents()
		{
			TezosManager.Instance.Wallet.EventManager.HandshakeReceived -= OnHandshakeReceived;
			TezosManager.Instance.Wallet.EventManager.WalletConnected -= OnWalletConnected;
			TezosManager.Instance.Wallet.EventManager.WalletDisconnected -= OnWalletDisconnected;
		}
	}

}