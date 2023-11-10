using TezosSDK.Beacon;
using TezosSDK.DesignPattern.Singleton;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos
{
	
	// [Serializable]
	// public class TezosManagerConfig
	// {
	// 	public NetworkType Network = NetworkType.ghostnet;
	// 	public string DefaultDAppName = "Tezos Unity SDK";
	// 	public string DefaultDAppUrl = "https://tezos.com/unity";
	// 	public string DefaultIconUrl = "https://unity.com/sites/default/files/2022-09/unity-tab-small.png";
	// 	public string RpcBaseUrl = $"https://{NetworkType.ghostnet.ToString()}.tezos.marigold.dev";
	// 	public int DefaultTimeoutSeconds = 45;
	// }

	public class TezosManager : SingletonMonoBehaviour<TezosManager>
	{
		[Header("App Configurations")]
		[SerializeField] private string appName = "Default App Name";
		[SerializeField] private string appUrl = "https://tezos.com";
		[SerializeField] private string appIcon = "https://tezos.com/favicon.ico";
		[SerializeField] private string appDescription = "App Description";
		
		[Tooltip("Logs will be printed to the console if the log level is equal or higher than this value.")]
		[SerializeField] private Logger.LogLevel logLevel = Logger.LogLevel.Debug;

		public DAppMetadata DAppMetadata { get; private set; } // TODO: this should be a property of Tezos class or WalletProvider class
		
		public ITezos Tezos { get; private set; }
		
		public IWalletProvider Wallet
		{
			get => Tezos?.Wallet;
		}

		public WalletMessageReceiver MessageReceiver
		{
			get => Wallet?.MessageReceiver;
		}

		protected override void Awake()
		{
			base.Awake();
			Init();
		}

		private void Init()
		{
			Logger.CurrentLogLevel = logLevel;

			DAppMetadata = new DAppMetadata
			{
				Name = appName,
				Url = appUrl,
				Icon = appIcon
			};
			
			Tezos = new Tezos(DAppMetadata);
		}
	}

}