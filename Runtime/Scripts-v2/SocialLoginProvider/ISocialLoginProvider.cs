using Tezos.Cysharp.Threading.Tasks;
using Tezos.Operation;

namespace Tezos.SocialLoginProvider
{
	public interface ISocialLoginProvider
	{
		public SocialLoginType       SocialLoginType { get; }
		UniTask                      Init(SocialLoginController socialLoginController);
		UniTask<SocialProviderData>  LogIn(SocialProviderData   socialLoginData);
		UniTask<bool>                LogOut();
		bool                         IsLoggedIn();
		UniTask<OperationResponse>   RequestOperation(OperationRequest                   operationRequest);
		UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest               signPayloadRequest);
		UniTask                      RequestContractOrigination(OriginateContractRequest originateContractRequest);
	}
}