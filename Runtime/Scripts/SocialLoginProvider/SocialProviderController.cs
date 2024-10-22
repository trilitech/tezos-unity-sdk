using System.Collections.Generic;
using System.Linq;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MessageSystem;
using Tezos.Operation;
using Tezos.Provider;
using Tezos.Reflection;
using Tezos.SaveSystem;

namespace Tezos.SocialLoginProvider
{
	public class SocialProviderController : IProviderController
	{
		private const string KEY_SOCIAL = "key-social-provider";

		private List<ISocialLoginProvider> _socialLoginProviders;
		private SocialProviderData         _socialProviderData;
		private SaveController             _saveController;

		public ProviderType ProviderType  => ProviderType.SOCIAL;
		public bool         IsConnected   => !string.IsNullOrEmpty(_socialProviderData?.WalletAddress);
		public bool         IsInitialized { get; private set; }

		public SocialProviderController(SaveController saveController) => _saveController = saveController;

		public async UniTask Initialize(IContext context)
		{
			_socialProviderData = await _saveController.Load<SocialProviderData>(KEY_SOCIAL);
#if UNITY_EDITOR || UNITY_ANDROID
			_socialLoginProviders = ReflectionHelper.CreateInstancesOfType<IAndroidProvider>().Cast<ISocialLoginProvider>().ToList();
#elif UNITY_IOS
			_socialLoginProviders = ReflectionHelper.CreateInstancesOfType<IiOSProvider>().Cast<ISocialLoginProvider>().ToList();
#elif UNITY_WEBGL
			_socialLoginProviders = ReflectionHelper.CreateInstancesOfType<IWebGLProvider>().Cast<ISocialLoginProvider>().ToList();
#else
			TezosLogger.LogError($"Unsupported platform:{Application.platform}");
#endif
			List<UniTask> initTasks = new();
			foreach (ISocialLoginProvider socialLoginProvider in _socialLoginProviders)
			{
				initTasks.Add(socialLoginProvider.Init(this));
			}

			await UniTask.WhenAll(initTasks);
			IsInitialized = true;
		}

		public SocialProviderData GetSocialProviderData() => _socialProviderData;

		public async UniTask<SocialProviderData> LogIn(SocialProviderData socialProviderData)
		{
			TezosLogger.LogInfo($"SocialLoginController::LogIn, provider count:{_socialLoginProviders.Count}");
			_socialProviderData = await _socialLoginProviders.Find(sp => sp.SocialLoginType == socialProviderData.SocialLoginType).LogIn(socialProviderData);
			_saveController.Save(KEY_SOCIAL, _socialProviderData);
			return _socialProviderData;
		}

		public async UniTask<bool> LogOut()
		{
			bool result = await _socialLoginProviders.Find(sp => sp.SocialLoginType == _socialProviderData.SocialLoginType).LogOut();
			_socialProviderData = null;
			_saveController.Delete(KEY_SOCIAL);
			return result;
		}

		public UniTask<string>              GetBalance()                                                  => _socialLoginProviders.Find(sp => sp.SocialLoginType == _socialProviderData?.SocialLoginType).GetBalance(_socialProviderData.WalletAddress);
		public UniTask<OperationResponse>   RequestOperation(OperationRequest     walletOperationRequest) => _socialLoginProviders.Find(sp => sp.SocialLoginType == _socialProviderData?.SocialLoginType).RequestOperation(walletOperationRequest);
		public UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest signPayloadRequest)     => _socialLoginProviders.Find(sp => sp.SocialLoginType == _socialProviderData?.SocialLoginType).RequestSignPayload(signPayloadRequest);
		public UniTask                      DeployContract(DeployContractRequest  deployContractRequest)  => _socialLoginProviders.Find(sp => sp.SocialLoginType == _socialProviderData?.SocialLoginType).RequestContractOrigination(deployContractRequest);
		public ISocialLoginProvider         GetSocialProvider<T>() where T : ISocialLoginProvider         => _socialLoginProviders.Find(p => p is T);
	}
}