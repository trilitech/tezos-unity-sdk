using System;
using System.Threading.Tasks;

namespace TezosSDK.SocialLoginProvider
{
	public interface ISocialLoginProvider
	{
		public event Action<SocialProviderData> SocialConnected;
		public event Action<SocialProviderData> SocialDisconnected;

		Task Init(SocialLoginController socialLoginController);
	}
}
