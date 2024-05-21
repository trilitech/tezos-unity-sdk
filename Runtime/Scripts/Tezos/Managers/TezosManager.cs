using Beacon.Sdk.Beacon.Permission;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.ScriptableObjects;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Connectors;
using UnityEngine;

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
		[SerializeField] private TezosLog.LogLevel logLevel = TezosLog.LogLevel.Debug;

		public static TezosManager Instance;

		public TezosConfigSO Config
		{
			get => config;
		}

		public DAppMetadata DAppMetadata { get; private set; }

		public WalletEventManager EventManager { get; private set; }

		public ITezos Tezos { get; private set; }

		public IWalletConnector WalletConnector { get; private set; }

		protected void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);

			ValidateConfig();
			CreateEventManager();
			InitializeTezos();
		}

		private void Start()
		{
			TezosLog.Debug("Tezos SDK initialized");
			EventManager.DispatchSDKInitializedEvent();
		}

		private void CreateEventManager()
		{
			var eventManager = FindObjectOfType<WalletEventManager>();
			
			if (!eventManager)
			{
				var eventManagerGO = new GameObject("WalletEventManager");
				EventManager = eventManagerGO.AddComponent<WalletEventManager>();
				DontDestroyOnLoad(eventManagerGO);
			}
			else
			{
				EventManager = eventManager;
			}
		}

		private void InitializeTezos()
		{
			TezosLog.SetLogLevel(logLevel);
			TezosLog.Info("Tezos SDK initializing...");
			
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