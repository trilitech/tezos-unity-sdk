using TezosSDK.Beacon;
using TezosSDK.DesignPattern.Singleton;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos
{
	public class TezosManager : MonoBehaviour
	{
		public static TezosManager Instance;
		
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

		public DAppMetadata DAppMetadata { get; private set; }

		public ITezos Tezos { get; private set; }

		public IWalletProvider Wallet
		{
			get => Tezos?.Wallet;
		}

		public WalletEventManager EventManager
		{
			get => _eventManager;
		}

		public IBeaconConnector BeaconConnector { get; private set; }

		public TezosConfigSO Config
		{
			get => config;
		}

		protected void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}
            
			Instance = this;
			CreateEventManager();
			InitializeTezos();
			DontDestroyOnLoad(gameObject);
		}

		private WalletEventManager _eventManager;

		private void InitializeTezos()
		{
			Logger.CurrentLogLevel = logLevel;

			Logger.LogDebug("Tezos SDK initializing...");
			
			DAppMetadata = new DAppMetadata(appName, appUrl, appIcon, appDescription);

			BeaconConnector = BeaconConnectorFactory.CreateConnector(Application.platform, config, _eventManager, DAppMetadata);
			
			Tezos = new Tezos(config, _eventManager, BeaconConnector);

			Logger.LogDebug("Tezos SDK initialized");
			EventManager.DispatchSDKInitializedEvent();
		}

		private void CreateEventManager()
		{
			// Create or get a WalletMessageReceiver Game object to receive callback messages
			var eventManager = GameObject.Find("WalletEventManager");

			_eventManager = eventManager != null
				? eventManager.GetComponent<WalletEventManager>()
				: new GameObject("WalletEventManager").AddComponent<WalletEventManager>();
			
			DontDestroyOnLoad(_eventManager);
		}
	}
}