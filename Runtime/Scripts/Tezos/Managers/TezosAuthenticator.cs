using TezosSDK.Helpers;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor; // Include UnityEditor namespace only when in the Unity Editor
#endif

namespace TezosSDK.Tezos.Managers
{

	public class TezosAuthenticator : MonoBehaviour
	{
		[SerializeField] private QrCodeGenerator qrCodeGenerator;
		[SerializeField] private GameObject deepLinkButton;
		[SerializeField] private GameObject socialLoginButton;
		[SerializeField] private GameObject logoutButton;
		[SerializeField] private Image darkBackground;

		private bool _isInitialized;

		// Platform flags to determine the current running platform
		private bool _isMobile;
		private bool _isWebGL;

		private WalletEventManager EventManager => TezosManager.Instance.EventManager;

		private IWalletConnection WalletConnection => TezosManager.Instance.Tezos.WalletConnection;

		private void Start()
		{
			Initialize(); // When the scene is loaded, initialize the SDK. But SDK might not be initialized yet.
			EventManager.SDKInitialized += OnSDKInitialized; // Subscribe to SDKInitialized event to initialize TezosAuthenticator if not yet initialized.
			darkBackground.gameObject.SetActive(true);
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

			if (TezosManager.Instance.WalletConnection.PairingRequestData != null)
			{
				qrCodeGenerator.SetQrCode(TezosManager.Instance.WalletConnection.PairingRequestData);
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

		private void OnPairingRequested(PairingRequestData pairingRequestData)
		{
			qrCodeGenerator.SetQrCode(pairingRequestData);
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

		public void ConnectByDeeplink() => WalletConnection.Connect(TezosManager.Instance.WalletConnection.ConnectorType);

		public void ConnectWithSocial() => WalletConnection.Connect(TezosManager.Instance.WalletConnection.ConnectorType);

		public void DisconnectWallet()
		{
			WalletConnection.Disconnect();
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

				// // Call Connect() only when on standalone
				// if (!_isWebGL && !_isMobile)
				// {
				// 	WalletConnection.Connect();
				// }
			}

			_isInitialized = true;
		}

		private void SetPlatformFlags()
		{
#if UNITY_EDITOR
			// When running in the Unity Editor, use the active build target to determine the platform
			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			_isMobile = buildTarget == BuildTarget.iOS || buildTarget == BuildTarget.Android;
			_isWebGL = buildTarget == BuildTarget.WebGL;
#else
			// When running outside of the Unity Editor (i.e., in a built application), use the runtime platform
			_isMobile = Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android;
			_isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
#endif
		}

		private void SubscribeToEvents()
		{
			// Subscribe to wallet events for handling user authentication.
			EventManager.PairingRequested += OnPairingRequested;
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
				darkBackground.gameObject.SetActive(false);
			}
			else
			{
				// Activate deepLinkButton when on mobile or WebGL, but not authenticated
				// deepLinkButton.SetActive(_isMobile || _isWebGL);

				// Activate socialLoginButton only when on WebGL and not authenticated
				socialLoginButton.SetActive(_isMobile || _isWebGL);

				// Activate qrCodePanel only on standalone and not authenticated
				// qrCodeGenerator.gameObject.SetActive(!_isMobile && !_isWebGL);
			}

			logoutButton.SetActive(isAuthenticated);
		}

		private void UnsubscribeFromEvents()
		{
			EventManager.PairingRequested -= OnPairingRequested;
			EventManager.WalletConnected -= OnWalletConnected;
			EventManager.WalletDisconnected -= OnWalletDisconnected;
		}
	}

}