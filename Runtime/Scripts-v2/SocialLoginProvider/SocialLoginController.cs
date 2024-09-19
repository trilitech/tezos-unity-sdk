using System.Collections.Generic;
using System.Linq;
using Tezos.Common;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MessageSystem;
using Tezos.Reflection;

namespace Tezos.WalletProvider
{
	public class SocialLoginController: IController
	{
		private List<ISocialLoginProvider> _socialLoginProviders;
		private IContext                   _context;
		private SocialProviderData         _socialProviderData;

		public bool IsInitialized { get; private set; }

		public async UniTask Initialize(IContext context)
		{
			_context = context;
			_socialLoginProviders = ReflectionHelper.CreateInstancesOfType<ISocialLoginProvider>().ToList();
			List<UniTask> initTasks = new();
			foreach (ISocialLoginProvider socialLoginProvider in _socialLoginProviders)
			{
				initTasks.Add(socialLoginProvider.Init(this));
			}
			
			await UniTask.WhenAll(initTasks);
			
			IsInitialized = true;
		}

		public bool IsSocialLoggedIn() => _socialLoginProviders.Find(sp => sp.SocialLoginType == _socialProviderData.SocialLoginType).IsLoggedIn();
		public SocialProviderData GetSocialProviderData() => _socialProviderData;

		public async UniTask<SocialProviderData> LogIn(SocialProviderData socialProviderData)
		{
			TezosLogger.LogInfo($"SocialLoginController::LogIn, provider count:{_socialLoginProviders.Count}");
			_socialProviderData = await _socialLoginProviders.Find(sp=>sp.SocialLoginType == socialProviderData.SocialLoginType).LogIn(socialProviderData);
			return _socialProviderData;
		}

		public async UniTask<bool> LogOut()
		{
			bool result = await _socialLoginProviders.Find(sp=>sp.SocialLoginType == _socialProviderData.SocialLoginType).LogOut();
			_socialProviderData = null;
			return result;
		}
	}
}
