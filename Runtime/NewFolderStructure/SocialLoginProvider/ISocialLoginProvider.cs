using System;
using System.Threading.Tasks;

namespace TezosSDK.WalletProvider
{
	public interface ISocialLoginProvider
	{
		public event Action<SocialProviderData> SocialConnected;
		public event Action<SocialProviderData> SocialDisconnected;

		Task Init(SocialLoginController socialLoginController);
		Task LogIn();
		Task LogOut();
	}
}
