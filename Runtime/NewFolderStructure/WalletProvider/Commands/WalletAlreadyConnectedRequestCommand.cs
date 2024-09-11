using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
    public class WalletAlreadyConnectedRequestCommand : ICommandMessage<bool>
    {
        public bool GetData() => false;
    }
}