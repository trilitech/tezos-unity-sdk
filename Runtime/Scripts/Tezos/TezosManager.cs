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
		
		
		public DAppMetadata DAppMetadata { get; private set; }

		public ITezos Tezos { get; private set; }
		
		public IWalletProvider Wallet
		{
			get => Tezos?.Wallet;
		}

		public WalletEventManager MessageReceiver
		{
			get => Wallet?.EventManager;
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
			
			DAppMetadata = new DAppMetadata(appName, appUrl, appIcon, appDescription);
			Tezos = new Tezos(DAppMetadata);
		}
	}
	
}