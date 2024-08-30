using System;
using System.Threading.Tasks;
using TezosSDK.MessageSystem;
using TezosSDK.SocialLoginProvider;
using TezosSDK.WalletProvider;

namespace TezosSDK.API
{
	public class TezosAPI
	{
		public static event Action<WalletProviderData> WalletConnected;
		public static event Action<WalletProviderData> WalletDisconnected;
		public static event Action<SocialProviderData> SocialLoginConnected;
		public static event Action<SocialProviderData> SocialLoginDisconnected;

		public static async Task<WalletProviderData> ConnectWallet(IContext context)
		{
			//todo: start wallet connection flow
			context.MessageSystem.InvokeMessage(new WalletConnectionRequestCommand());

			return new WalletProviderData
			{
				WalletAddress = "test",
				Network       = "test"
			};
		}

		public static async Task<SocialProviderData> SocialLogin()
		{
			//todo: start social login flow

			return new SocialProviderData
			{
				WalletAddress = "test",
				LoginDetails  = "test@gmail.com",
				LoginType     = "gmail"
			};
		}
	}
}
