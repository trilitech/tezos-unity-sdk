using System.Threading.Tasks;
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
		[SerializeField] private TezosLogger.LogLevel logLevel = TezosLogger.LogLevel.Debug;

		public static TezosManager Instance;

		public TezosConfigSO Config
		{
			get => config;
		}

		public DAppMetadata DAppMetadata { get; private set; }

		public WalletEventManager EventManager { get; private set; }

		public bool IsInitialized { get; private set; }

		public   ITezos            Tezos            { get; private set; }
		public   IWalletConnection WalletConnection => Tezos.WalletConnection;

		protected async void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);

			ValidateConfig();
			InitializeTezos();
		}

		private async void InitializeTezos()
		{
			TezosLogger.SetLogLevel(logLevel);
			TezosLogger.LogInfo("Tezos SDK initializing...");

			EventManager = new WalletEventManager();
			DAppMetadata = new DAppMetadata(appName, appUrl, appIcon, appDescription);
			var walletProvider = new WalletProvider(EventManager);
			Tezos = new Core.Tezos(config, walletProvider);
			await InitializeConnectors();
			IsInitialized = true;

			TezosLogger.LogInfo("Tezos SDK initialized.");
			EventManager.DispatchSDKInitializedEvent();
		}

		private async Task InitializeConnectors()
		{
			Task webGLTask = WalletConnectorFactory.GetConnector(ConnectorType.BeaconWebGl).InitializeAsync(EventManager);
			Task dotNetTask = WalletConnectorFactory.GetConnector(ConnectorType.BeaconDotNet).InitializeAsync(EventManager);
			Task kukaiTask = WalletConnectorFactory.GetConnector(ConnectorType.Kukai).InitializeAsync(EventManager);

			await Task.WhenAll(webGLTask, dotNetTask, kukaiTask);
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
				Debug.LogWarning("Pinata API key is not set in TezosConfigSO. You will not be able to upload images to IPFS.");
			}

			if (Config.DataProvider == null)
			{
				Debug.LogError("Data provider is not set in TezosConfigSO. You will not be able to query data.");
			}
		}
	}

}