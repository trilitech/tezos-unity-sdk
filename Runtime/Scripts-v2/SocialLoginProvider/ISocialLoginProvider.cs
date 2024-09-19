using Tezos.Cysharp.Threading.Tasks;

namespace Tezos.WalletProvider
{
	public interface ISocialLoginProvider
	{
		public SocialLoginType SocialLoginType { get; }
		UniTask Init(SocialLoginController socialLoginController);
		UniTask<SocialProviderData> LogIn(SocialProviderData socialLoginData);
		UniTask<bool> LogOut();
		bool IsLoggedIn();
	}
}
