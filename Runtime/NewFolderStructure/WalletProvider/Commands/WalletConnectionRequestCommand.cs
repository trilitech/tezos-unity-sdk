using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public class WalletConnectionRequestCommand : ICommandMessage<WalletProviderData>
	{
		private WalletProviderData _walletProviderData;
		public WalletConnectionRequestCommand(WalletProviderData walletProviderData) => _walletProviderData = walletProviderData;
		public WalletProviderData GetData() => _walletProviderData;
	}
}
