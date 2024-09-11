using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
    public class SocialAlreadyLoggedInRequestCommand : ICommandMessage<bool>
    {
        public bool GetData() => false;
    }
}