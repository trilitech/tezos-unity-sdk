using System;
using System.Threading.Tasks;

namespace TezosSDK.WalletProvider
{
	public interface IWalletProvider
	{
		public event Action<WalletProviderData> WalletConnected;
		public event Action<WalletProviderData> WalletDisconnected;

		Task Init(WalletProviderController walletProviderController);
	}
}
