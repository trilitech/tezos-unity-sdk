using Tezos.Common;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Operation;

namespace Tezos.Provider
{
	public interface IProviderController : IController
	{
		ProviderType                 ProviderType { get; }
		bool                         IsConnected  { get; }
		UniTask<OperationResponse>   RequestOperation(OperationRequest                   operationRequest);
		UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest               signRequest);
		UniTask                      RequestContractOrigination(OriginateContractRequest originationRequest);
	}
}