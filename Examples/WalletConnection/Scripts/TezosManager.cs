using TezosSDK.Beacon;
using TezosSDK.DesignPattern.Singleton;
using TezosSDK.Tezos.Wallet;
using TezosSDK.View;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos
{
	public class TezosManager : SingletonMonoBehaviour<TezosManager>
	{
		[SerializeField] private AuthenticationManager authManager;
		
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
			authManager.UseTezos(Tezos);
		}
	}

}