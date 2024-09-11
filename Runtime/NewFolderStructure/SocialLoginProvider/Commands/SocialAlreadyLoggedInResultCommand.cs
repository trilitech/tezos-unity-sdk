using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
    public class SocialAlreadyLoggedInResultCommand : ICommandMessage<bool>
    {
        private bool _isConnected = false;
        
        public SocialAlreadyLoggedInResultCommand(bool isConnected) => _isConnected = isConnected;
        
        public bool GetData() => _isConnected;
    }
}