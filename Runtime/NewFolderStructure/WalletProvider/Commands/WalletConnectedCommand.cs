using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public class WalletConnectedCommand : ICommandMessage<WalletProviderData>
	{
		private WalletProviderData _walletProviderData;

		public WalletConnectedCommand(WalletProviderData walletProviderData) => _walletProviderData = walletProviderData;

		public WalletProviderData GetData() => _walletProviderData;
	}
}
