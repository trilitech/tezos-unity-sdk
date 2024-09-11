using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
    public class SocialLoggedInCommand : ICommandMessage<SocialProviderData>
    {
        private SocialProviderData _socialProviderData;

        public SocialLoggedInCommand(SocialProviderData socialProviderData) => _socialProviderData = socialProviderData;
        
        public SocialProviderData GetData() => _socialProviderData;
    }
}