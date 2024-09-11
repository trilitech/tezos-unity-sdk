using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public class WalletDisconnectionRequestCommand : ICommandMessage<WalletProviderData>
	{
		private WalletProviderData _walletProviderData;
		public WalletDisconnectionRequestCommand(WalletProviderData walletProviderData) => _walletProviderData = walletProviderData;
		public WalletProviderData GetData() => _walletProviderData;
	}
}
