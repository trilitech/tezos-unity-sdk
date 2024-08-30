using System;
using System.Threading.Tasks;

namespace TezosSDK.WalletProvider
{
	public class BeaconProvider : IWalletProvider
	{
		public event Action<WalletProviderData> WalletConnected;
		public event Action<WalletProviderData> WalletDisconnected;

		public Task Init(WalletProviderController walletProviderController)
		{
			return Task.CompletedTask;
		}

		public Task Connect()
		{
			return Task.CompletedTask;
		}

		public Task Disconnect()
		{
			return Task.CompletedTask;
		}
	}
}
