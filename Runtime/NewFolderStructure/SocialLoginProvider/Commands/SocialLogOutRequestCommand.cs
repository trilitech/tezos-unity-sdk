using TezosSDK.MessageSystem;

namespace TezosSDK.WalletProvider
{
    public class SocialLogOutRequestCommand : ICommandMessage<SocialProviderData>
    {
        private SocialProviderData _socialProviderData;

        public SocialLogOutRequestCommand(SocialProviderData socialProviderData) => _socialProviderData = socialProviderData;
        
        public SocialProviderData GetData() => _socialProviderData;
    }
}