using System.Collections.Generic;
using Beacon.Sdk.Beacon.Permission;
using Tezos.API;
using Tezos.MessageSystem;
using Tezos.Configs;
using Tezos.Logger;
using Tezos.MainThreadDispatcher;
using Tezos.Provider;
using Tezos.SaveSystem;
using Tezos.SocialLoginProvider;
using Tezos.WalletProvider;
using UnityEngine;

namespace Tezos.Initializer
{
	public class TezosInitializer : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static async void Initialize()
		{
			TezosLogger.LogDebug($"Tezos SDK starting to initialize");
			UnityMainThreadDispatcher unityMainThreadDispatcher = new GameObject("UnityMainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();

			Context                  context                  = new();
			SaveController           saveController           = new();
			SocialProviderController socialLoginController    = new(saveController);
			WalletProviderController walletProviderController = new(saveController);

			unityMainThreadDispatcher.gameObject.hideFlags = HideFlags.HideAndDontSave;

			ValidateConfig();

			TezosLogger.LogInfo($"Initializing ProviderFactory");
			ProviderFactory.Init(
			                     new List<IProviderController>
			                     {
				                     socialLoginController,
				                     walletProviderController
			                     }
			                    );
			TezosLogger.LogInfo($"ProviderFactory Initialized");
			TezosLogger.LogInfo($"Initializing SaveController");
			await saveController.Initialize(context);
			TezosLogger.LogInfo($"SaveController Initialized");
			TezosLogger.LogInfo($"Initializing TezosAPI");
			TezosAPI.Init(context, walletProviderController, socialLoginController);
			TezosLogger.LogInfo($"TezosAPI Initialized");
			TezosLogger.LogInfo($"Initializing SocialLoginController");
			await socialLoginController.Initialize(context);
			TezosLogger.LogInfo($"SocialLoginController Initialized");
			TezosLogger.LogInfo($"Initializing WalletProviderController");
			await walletProviderController.Initialize(context);
			TezosLogger.LogInfo($"WalletProviderController Initialized");

			context.MessageSystem.InvokeMessage(new SdkInitializedCommand());

			TezosLogger.LogInfo($"Tezos SDK initialized");
		}

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

			if (string.IsNullOrEmpty(config.PinataApiToken))
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