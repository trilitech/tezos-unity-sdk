using System;
using System.Threading.Tasks;
using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public interface IWalletProvider
	{
		public event Action<WalletProviderData> WalletConnected;
		public event Action<WalletProviderData> WalletDisconnected;
		public WalletType WalletType { get; }
		Task Init(IContext context);
		Task Connect(WalletProviderData data);
		Task Disconnect();
		bool IsAlreadyConnected();
	}
}
