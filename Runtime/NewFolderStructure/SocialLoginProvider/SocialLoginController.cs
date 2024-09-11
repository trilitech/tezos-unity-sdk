using System.Collections.Generic;
using System.Threading.Tasks;
using TezosSDK.Reflection;
using TezosSDK.Common;
using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public class SocialLoginController: IController
	{
		private IEnumerable<ISocialLoginProvider> _socialLoginProviders;
		private IContext                          _context;

		public bool IsInitialized { get; private set; }

		public async Task Initialize(IContext context)
		{
			_context = context;
			_socialLoginProviders = ReflectionHelper.CreateInstancesOfType<ISocialLoginProvider>();
			List<Task> initTasks = new();
			foreach (ISocialLoginProvider socialLoginProvider in _socialLoginProviders)
			{
				socialLoginProvider.SocialConnected    += OnSocialConnected;
				socialLoginProvider.SocialDisconnected += OnSocialDisconnected;
				initTasks.Add(socialLoginProvider.Init(this));
			}
			
			_context.MessageSystem.AddListener<SocialLogInRequestCommand>(OnSocialLogInRequested);
			_context.MessageSystem.AddListener<SocialLogOutRequestCommand>(OnSocialLogOutRequested);

			await Task.WhenAll(initTasks);
			
			IsInitialized = true;
		}

		private void OnSocialLogInRequested(SocialLogInRequestCommand socialLogInRequestCommand)
		{
			foreach (var socialLoginProvider in _socialLoginProviders)
			{
				socialLoginProvider.LogIn();
			}
		}

		private void OnSocialLogOutRequested(SocialLogOutRequestCommand socialLogOutRequestCommand)
		{
			foreach (var socialLoginProvider in _socialLoginProviders)
			{
				socialLoginProvider.LogOut();
			}
		}

		private void OnSocialConnected(SocialProviderData socialProviderData)    => _context.MessageSystem.InvokeMessage(new SocialLoggedInCommand(socialProviderData));
		private void OnSocialDisconnected(SocialProviderData socialProviderData) => _context.MessageSystem.InvokeMessage(new SocialLoggedOutCommand(socialProviderData));
	}
}
