using System;
using System.Threading.Tasks;

namespace TezosSDK.SocialLoginProvider
{
	public class KukaiProvider : ISocialLoginProvider
	{
		public event Action<SocialProviderData> SocialConnected;
		public event Action<SocialProviderData> SocialDisconnected;
		
		public Task Init(SocialLoginController socialLoginController)
		{
			return Task.CompletedTask;
		}
	}
}
