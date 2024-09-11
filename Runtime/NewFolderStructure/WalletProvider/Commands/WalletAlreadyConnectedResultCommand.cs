using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
    public class WalletAlreadyConnectedResultCommand : ICommandMessage<bool>
    {
        private bool _isConnected = false;
        
        public WalletAlreadyConnectedResultCommand(bool isConnected) => _isConnected = isConnected;
        
        public bool GetData() => _isConnected;
    }
}