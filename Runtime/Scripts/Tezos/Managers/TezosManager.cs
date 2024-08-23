using System.Threading.Tasks;
using Beacon.Sdk.Beacon.Permission;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Connectors;
using UnityEngine;

namespace TezosSDK.Tezos.Managers
{

	public class TezosManager
	{
		[Tooltip("Logs will be printed to the console if the log level is equal or higher than this value.")]
		[SerializeField] private TezosLogger.LogLevel logLevel = TezosLogger.LogLevel.Debug;

		public static TezosManager Instance;
		
		public WalletEventManager EventManager { get; private set; }

		public bool IsInitialized { get; private set; }

		public   ITezos            Tezos            { get; private set; }
		public   IWalletConnection WalletConnection => Tezos.WalletConnection;

		protected void Awake()
		{
			ValidateConfig();
			InitializeTezos();
		}

		private async void InitializeTezos()
		{
			TezosLogger.SetLogLevel(logLevel);
			TezosLogger.LogInfo("Tezos SDK initializing...");

			EventManager = new WalletEventManager();
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
	}

}