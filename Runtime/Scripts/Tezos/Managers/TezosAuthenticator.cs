using TezosSDK.Helpers;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using UnityEngine;

namespace TezosSDK.Tezos.Managers
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
		
		private bool _isInitialized;

		private ITezos Tezos { get; set; }

		private IWalletConnection WalletConnection
		{
			get => TezosManager.Instance.Tezos.WalletConnection;
		}

		private WalletEventManager EventManager
		{
			get => TezosManager.Instance.EventManager;
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
			if (!TezosManager.Instance)
			{
				return;
			}

			if (!TezosManager.Instance.IsInitialized)
			{
				return;
			}

			if (WalletConnection.IsConnected)
			{
				return;
			}

			if (WalletConnection.HandshakeData != null)
			{
				qrCodeGenerator.SetQrCode(WalletConnection.HandshakeData);
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

		private void Start()
		{
			Initialize(); // When the scene is loaded, initialize the SDK. But SDK might not be initialized yet.
			EventManager.SDKInitialized += OnSDKInitialized; // Subscribe to SDKInitialized event to initialize TezosAuthenticator if not yet initialized.
			Tezos = TezosManager.Instance.Tezos;
		}
		
		private void Initialize()
		{
			if (_isInitialized)
			{
				return;
			}
			
			if (!TezosManager.Instance || !TezosManager.Instance.IsInitialized)
			{
				// TezosManager is not initialized yet. Wait for it to be initialized.
				// We should be subscribed to SDKInitialized event to initialize TezosAuthenticator when TezosManager is initialized.
				return;
			}
			
			SubscribeToEvents();
			SetPlatformFlags();

			if (WalletConnection.IsConnected)
			{
				ToggleUIElements(true);
			}
			else
			{
				ToggleUIElements(false);

				// Call Connect() only when on standalone
				if (!_isWebGL && !_isMobile)
				{
					WalletConnection.Connect(WalletProviderType.beacon);
				}
			}
			
			_isInitialized = true;
		}

		private void OnSDKInitialized()
		{
			TezosLogger.LogDebug("TezosAuthenticator.OnSDKInitialized");

			if (!_isInitialized)
			{
				Initialize();
			}
		}

		private void OnWalletConnected(WalletInfo walletInfo)
		{
			TezosLogger.LogDebug("TezosAuthenticator.OnWalletConnected");
			ToggleUIElements(true);
		}

		private void OnWalletDisconnected(WalletInfo walletInfo)
		{
			TezosLogger.LogDebug("TezosAuthenticator.OnWalletDisconnected");
			ToggleUIElements(false);
		}

		public void ConnectByDeeplink()
		{
			WalletConnection.Connect(WalletProviderType.beacon);
		}

		public void ConnectWithSocial()
		{
			WalletConnection.Connect(WalletProviderType.kukai);
		}

		public void DisconnectWallet()
		{
			WalletConnection.Disconnect();
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
			EventManager.HandshakeReceived += OnHandshakeReceived;
			EventManager.WalletConnected += OnWalletConnected;
			EventManager.WalletDisconnected += OnWalletDisconnected;
			EventManager.SDKInitialized += OnSDKInitialized;
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
			EventManager.HandshakeReceived -= OnHandshakeReceived;
			EventManager.WalletConnected -= OnWalletConnected;
			EventManager.WalletDisconnected -= OnWalletDisconnected;
		}
	}

}