using TezosSDK.Beacon;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
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
		
		[Tooltip("Logs will be printed to the console if the log level is equal or higher than this value.")]
		[SerializeField] private Logger.LogLevel logLevel = Logger.LogLevel.Debug;
		[Tooltip("Create API key in Pinata service https://app.pinata.cloud/developers/api-keys and paste JWT value " +
		         "here to be able to upload images to IPFS.")]
		[SerializeField] private string pinataApiKey;
		public DAppMetadata DAppMetadata { get; private set; }

		public ITezos Tezos { get; private set; }

		public IWalletProvider Wallet => Tezos?.Wallet;

		public WalletEventManager MessageReceiver => Wallet?.EventManager;

		public static string PinataApiKey => TezosConfig.Instance.pinataApiKey;

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
			Tezos = new Tezos(DAppMetadata);
		}
	}
}