using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezosSDK.Reflection;
using TezosSDK.Common;
using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public class SocialLoginController: IController
	{
		private List<ISocialLoginProvider> _socialLoginProviders;
		private IContext                   _context;
		private SocialProviderData         _socialProviderData;

		public bool IsInitialized { get; private set; }

		public async Task Initialize(IContext context)
		{
			_context = context;
			_socialLoginProviders = ReflectionHelper.CreateInstancesOfType<ISocialLoginProvider>().ToList();
			List<Task> initTasks = new();
			foreach (ISocialLoginProvider socialLoginProvider in _socialLoginProviders)
			{
				initTasks.Add(socialLoginProvider.Init(this));
			}
			
			await Task.WhenAll(initTasks);
			
			IsInitialized = true;
		}

		public async Task<SocialProviderData> LogIn(SocialProviderData socialProviderData)
		{
			_socialProviderData = await _socialLoginProviders.Find(sp=>sp.SocialLoginType == socialProviderData.SocialLoginType).LogIn(socialProviderData);
			return _socialProviderData;
		}

		public async Task<bool> LogOut()
		{
			bool result = await _socialLoginProviders.Find(sp=>sp.SocialLoginType == _socialProviderData.SocialLoginType).LogOut();
			_socialProviderData = null;
			return result;
		}
	}
}
