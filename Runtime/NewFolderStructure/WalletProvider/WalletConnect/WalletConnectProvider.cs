using System;
using System.Threading.Tasks;

namespace TezosSDK.WalletProvider
{
	public class WalletConnectProvider: IWalletProvider
	{
		public event Action<WalletProviderData> WalletConnected;
		public event Action<WalletProviderData> WalletDisconnected;

		public Task Init(WalletProviderController walletProviderController)
		{
			return Task.CompletedTask;
		}
	}
}
