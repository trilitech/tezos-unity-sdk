using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
    public class SocialLogInRequestCommand : ICommandMessage<SocialProviderData>
    {
        private SocialProviderData _socialProviderData;

        public SocialLogInRequestCommand(SocialProviderData socialProviderData) => _socialProviderData = socialProviderData;
        
        public SocialProviderData GetData() => _socialProviderData;
    }
}