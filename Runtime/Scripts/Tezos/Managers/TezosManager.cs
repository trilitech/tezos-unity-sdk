using System;
using System.Threading.Tasks;
using Beacon.Sdk.Beacon.Permission;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.ScriptableObjects;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Connectors;
using TezosSDK.WalletServices.Connectors.DotNet;
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

		public ITezos Tezos { get; private set; }

		public IWalletConnector WalletConnector { get; private set; }
		
		public bool IsInitialized { get; private set; }

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
			EventManager = new WalletEventManager();
			await InitializeTezosAsync();
		}

		private async Task InitializeTezosAsync()
		{
			TezosLogger.SetLogLevel(logLevel);
			TezosLogger.LogInfo("Tezos SDK initializing...");
			
			DAppMetadata = new DAppMetadata(appName, appUrl, appIcon, appDescription);
			WalletConnector = WalletConnectorFactory.CreateConnector(ConnectorType.Kukai, EventManager);
			var walletProvider = new WalletProvider(EventManager, WalletConnector);
			Tezos = new Core.Tezos(config, walletProvider);
			
			await WalletConnector.InitializeAsync();
			IsInitialized = true;

			TezosLogger.LogInfo("Tezos SDK initialized.");
			EventManager.DispatchSDKInitializedEvent();
		}

		private void OnDestroy()
		{
			WalletConnector?.Dispose();
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