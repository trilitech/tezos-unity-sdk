using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public class WalletConnectionRequestCommand : ICommandMessage<bool>
	{
		public bool GetData() => true;
	}
}
