using System.Collections.Generic;
using System.Threading.Tasks;
using TezosSDK.Reflection;
using TezosSDK.Common;
using TezosSDK.MessageSystem;

namespace TezosSDK.SocialLoginProvider
{
	public class SocialLoginController: IController
	{
		private IEnumerable<ISocialLoginProvider> _socialLoginProviders;

		public bool     IsInitialized { get; }
		public IContext Context       { get; }

		public async Task Initialize(IContext context)
		{
			_socialLoginProviders = ReflectionHelper.CreateInstancesOfType<ISocialLoginProvider>();
			List<Task> initTasks = new();
			foreach (ISocialLoginProvider socialLoginProvider in _socialLoginProviders)
			{
				socialLoginProvider.SocialConnected    += OnSocialConnected;
				socialLoginProvider.SocialDisconnected += OnSocialDisconnected;
				initTasks.Add(socialLoginProvider.Init(this));
			}

			await Task.WhenAll(initTasks);
		}

		private void OnSocialConnected(SocialProviderData    socialProviderData) { }
		private void OnSocialDisconnected(SocialProviderData obj)                { }
	}
}
