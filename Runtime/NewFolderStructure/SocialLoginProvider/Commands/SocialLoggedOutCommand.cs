using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
    public class SocialLoggedOutCommand : ICommandMessage<SocialProviderData>
    {
        private SocialProviderData _socialProviderData;

        public SocialLoggedOutCommand(SocialProviderData socialProviderData) => _socialProviderData = socialProviderData;
        
        public SocialProviderData GetData() => _socialProviderData;
    }
}