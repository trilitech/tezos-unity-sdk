using Beacon.Sdk.Beacon.Permission;
using TezosSDK.Beacon;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos
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

		public IBeaconConnector BeaconConnector { get; private set; }

		public TezosConfigSO Config
		{
			get => config;
		}

		public DAppMetadata DAppMetadata { get; private set; }

		public WalletEventManager EventManager { get; private set; }

		public ITezos Tezos { get; private set; }

		public IWalletProvider Wallet
		{
			get => Tezos?.Wallet;
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

			BeaconConnector = BeaconConnectorFactory.CreateConnector(Application.platform, EventManager);

			Tezos = new Tezos(config, EventManager, BeaconConnector);

			Logger.LogDebug("Tezos SDK initialized");
			EventManager.DispatchSDKInitializedEvent();
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