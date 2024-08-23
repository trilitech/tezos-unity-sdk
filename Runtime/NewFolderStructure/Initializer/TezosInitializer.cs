using Beacon.Sdk.Beacon.Permission;
using TezosSDK.MessageSystem;
using TezosSDK.Configs;
using TezosSDK.SocialLoginProvider;
using TezosSDK.WalletProvider;
using UnityEngine;

namespace Tezos.Initializer
{
	public static class TezosInitializer
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static async void Initialize()
		{
			//Create all required MVC components here and define dependencies aswell
			//for simplier cases, we can directly order the init manually here and resolve dependencies
			//for more complex cases, we can use DI (dependency injection)
			//Dependency injection takes into account of classes and their dependencies and order and resolve the initializations based on that

			Context                  context                  = new();
			SocialLoginController    socialLoginController    = new();
			WalletProviderController walletProviderController = new();

			await socialLoginController.Initialize(context);
			await walletProviderController.Initialize(context);

			context.MessageSystem.InvokeMessage(new SdkInitializedCommand());
		}

		// private static async void InitializeTezos()
		// {
		//     TezosLogger.SetLogLevel(logLevel);
		//     TezosLogger.LogInfo("Tezos SDK initializing...");
		//
		//     EventManager = new WalletEventManager();
		//     DAppMetadata = new DAppMetadata(appName, appUrl, appIcon, appDescription);
		//     var walletProvider = new WalletProvider(EventManager);
		//     Tezos = new Core.Tezos(config, walletProvider);
		//     await InitializeConnectors();
		//     IsInitialized = true;
		//
		//     TezosLogger.LogInfo("Tezos SDK initialized.");
		//     EventManager.DispatchSDKInitializedEvent();
		// }
		//
		// private static async Task InitializeConnectors()
		// {
		//     Task webGLTask = WalletConnectorFactory.GetConnector(ConnectorType.BeaconWebGl).InitializeAsync(EventManager);
		//     Task dotNetTask = WalletConnectorFactory.GetConnector(ConnectorType.BeaconDotNet).InitializeAsync(EventManager);
		//     Task kukaiTask = WalletConnectorFactory.GetConnector(ConnectorType.Kukai).InitializeAsync(EventManager);
		//
		//     await Task.WhenAll(webGLTask, dotNetTask, kukaiTask);
		// }

		private static void ValidateConfig()
		{
			TezosConfig config = ConfigGetter.GetOrCreateConfig<TezosConfig>();

			if (config.Network == NetworkType.mainnet)
			{
				Debug.LogWarning("You are using Mainnet. Make sure you are not using Mainnet for testing purposes.");
			}

			if (config.Network != config.DataProvider.Network)
			{
				Debug.LogError("Networks for RPC and Data Provider are different. Make sure they are the same.");
			}

			if (string.IsNullOrEmpty(config.PinataApiKey))
			{
				Debug.LogWarning("Pinata API key is not set in TezosConfigSO. You will not be able to upload images to IPFS.");
			}

			if (config.DataProvider == null)
			{
				Debug.LogError("Data provider is not set in TezosConfigSO. You will not be able to query data.");
			}
		}
	}
}
