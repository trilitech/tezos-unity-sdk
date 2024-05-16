using Beacon.Sdk.Beacon.Permission;
using TezosSDK.Tezos.Interfaces;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.ScriptableObjects;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Connectors;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logging.Logger;

namespace TezosSDK.Tezos.Managers
{

	public class TezosManager : MonoBehaviour
	{
		[Header("App Configurations")]
		[SerializeField] private string appName = "Default App Name";
		[SerializeField] private string appUrl = "https://tezos.com";
		[SerializeField] private string appIcon = "https://tezos.com/favicon.ico";
		[SerializeField] private string appDescription = "App Description";

		[Space(20)]
		[Header("SDK Configuration")]
		[SerializeField] private TezosConfigSO config;

		[Tooltip("Logs will be printed to the console if the log level is equal or higher than this value.")]
		[SerializeField] private Logger.LogLevel logLevel = Logger.LogLevel.Debug;
		public static TezosManager Instance;

		public TezosConfigSO Config
		{
			get => config;
		}

		public DAppMetadata DAppMetadata { get; private set; }

		public WalletEventManager EventManager { get; private set; }

		public ITezos Tezos { get; private set; }

		public IWalletAccount WalletAccount
		{
			get => Tezos?.WalletAccount;
		}

		public IWalletConnection WalletConnection
		{
			get => Tezos?.WalletConnection;
		}

		public IWalletConnector WalletConnector { get; private set; }

		public IWalletContract WalletContract
		{
			get => Tezos?.WalletContract;
		}

		public IWalletEventProvider WalletEventProvider
		{
			get => Tezos?.WalletEventProvider;
		}

		public IWalletTransaction WalletTransaction
		{
			get => Tezos?.WalletTransaction;
		}

		protected void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;

			ValidateConfig();
			CreateEventManager();
			InitializeTezos();
			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			Logger.LogDebug("Tezos SDK initialized");
			EventManager.DispatchSDKInitializedEvent();
		}

		private void CreateEventManager()
		{
			// Create or get a WalletMessageReceiver Game object to receive callback messages
			var eventManager = GameObject.Find("WalletEventManager");

			EventManager = eventManager != null
				? eventManager.GetComponent<WalletEventManager>()
				: new GameObject("WalletEventManager").AddComponent<WalletEventManager>();

			DontDestroyOnLoad(EventManager);
		}

		private void InitializeTezos()
		{
			Logger.CurrentLogLevel = logLevel;
			Logger.LogDebug("Tezos SDK initializing...");
			DAppMetadata = new DAppMetadata(appName, appUrl, appIcon, appDescription);
			WalletConnector = WalletConnectorFactory.CreateConnector(Application.platform, EventManager);
			var walletProvider = new WalletProvider(EventManager, WalletConnector);
			Tezos = new Core.Tezos(config, walletProvider);
		}

		private void ValidateConfig()
		{
			if (Config.Network == NetworkType.mainnet)
			{
				Debug.LogWarning("You are using Mainnet. Make sure you are not using Mainnet for testing purposes.");
			}

			if (Config.Network != Config.DataProvider.Network)
			{
				Debug.LogError("Networks for RPC and Data Provider are different. Make sure they are the same.");
			}

			if (string.IsNullOrEmpty(Config.PinataApiKey))
			{
				Debug.LogWarning(
					"Pinata API key is not set in TezosConfigSO. You will not be able to upload images to IPFS.");
			}

			if (Config.DataProvider == null)
			{
				Debug.LogError("Data provider is not set in TezosConfigSO. You will not be able to query data.");
			}
		}
	}

}