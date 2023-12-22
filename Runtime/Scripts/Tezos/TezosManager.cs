using TezosSDK.Beacon;
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
		
		[Tooltip("Logs will be printed to the console if the log level is equal or higher than this value.")]
		[SerializeField] private Logger.LogLevel logLevel = Logger.LogLevel.Debug;
		
		[Tooltip("Create API key in Pinata service https://app.pinata.cloud/developers/api-keys and paste JWT value " +
		         "here to be able to upload images to IPFS.")]
		[SerializeField] private string pinataApiKey;
		
		[Tooltip("Should we open the wallet app on mobiles after connect?")]
		[SerializeField] private bool redirectToWallet = true;
		
		public bool IsInitialized { get; private set; }

		public DAppMetadata DAppMetadata { get; private set; }

		public ITezos Tezos { get; private set; }

		public IWalletProvider Wallet
		{
			get => Tezos?.Wallet;
		}

		public WalletEventManager EventManager
		{
			get => Wallet?.EventManager;
		}

		public static string PinataApiKey
		{
			get => TezosConfig.Instance.pinataApiKey;
		}

		protected void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}
            
			Instance = this;
			InitializeTezos();
			DontDestroyOnLoad(gameObject);
		}

		private void InitializeTezos()
		{
			Logger.CurrentLogLevel = logLevel;
			TezosConfig.Instance.pinataApiKey = pinataApiKey;
			
			DAppMetadata = new DAppMetadata(appName, appUrl, appIcon, appDescription);
			Tezos = new Tezos(DAppMetadata, redirectToWallet);
			
			IsInitialized = true;
			EventManager.DispatchSDKInitializedEvent();
		}
	}
}