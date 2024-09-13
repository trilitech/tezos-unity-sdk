using System.Threading.Tasks;

namespace TezosSDK.WalletProvider
{
	public interface ISocialLoginProvider
	{
		public SocialLoginType SocialLoginType { get; }
		Task Init(SocialLoginController socialLoginController);
		Task<SocialProviderData> LogIn(SocialProviderData socialLoginData);
		Task<bool> LogOut();
	}
}
