using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public class WalletDisconnectedCommand : ICommandMessage<WalletProviderData>
	{
		private WalletProviderData _walletProviderData;

		public WalletDisconnectedCommand(WalletProviderData walletProviderData) => _walletProviderData = walletProviderData;
		
		public WalletProviderData GetData() => _walletProviderData;
	}
}
