using Tezos.Cysharp.Threading.Tasks;
using Tezos.Operation;

namespace Tezos.SocialLoginProvider
{
	public interface ISocialLoginProvider
	{
		public SocialLoginType       SocialLoginType { get; }
		UniTask                      Init(SocialProviderController socialLoginController);
		UniTask<SocialProviderData>  LogIn(SocialProviderData      socialLoginData);
		UniTask<bool>                LogOut();
		UniTask<string>              GetBalance(string walletAddress);
		bool                         IsLoggedIn();
		UniTask<OperationResponse>   RequestOperation(OperationRequest                   operationRequest);
		UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest               signPayloadRequest);
		UniTask                      RequestContractOrigination(DeployContractRequest deployContractRequest);
	}
}