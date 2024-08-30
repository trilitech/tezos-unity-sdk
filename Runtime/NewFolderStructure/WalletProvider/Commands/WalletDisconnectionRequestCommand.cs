using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
	public class WalletDisconnectionRequestCommand : ICommandMessage<bool>
	{
		public bool GetData() => true;
	}
}
