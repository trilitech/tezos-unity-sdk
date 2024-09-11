using Beacon.Sdk.Beacon.Permission;
using TezosSDK.API;
using TezosSDK.MessageSystem;
using TezosSDK.Configs;
using TezosSDK.WalletProvider;
using UnityEngine;

namespace Tezos.Initializer
{
	public static class TezosInitializer
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static async void Initialize()
		{
			Context                  context                  = new();
			SocialLoginController    socialLoginController    = new();
			WalletProviderController walletProviderController = new();

			ValidateConfig();
			
			TezosAPI.Init(context);
			await socialLoginController.Initialize(context);
			await walletProviderController.Initialize(context);

			context.MessageSystem.InvokeMessage(new SdkInitializedCommand());
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
