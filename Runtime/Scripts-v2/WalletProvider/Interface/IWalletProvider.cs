using System;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.MessageSystem;
using Tezos.Operation;
using OperationRequest = Tezos.Operation.OperationRequest;
using OperationResponse = Tezos.Operation.OperationResponse;

namespace Tezos.WalletProvider
{
	public interface IWalletProvider
	{
		public event Action<WalletProviderData> WalletConnected;
		public event Action                     WalletDisconnected;
		public event Action<string>             PairingRequested;
		public WalletType                       WalletType { get; }
		UniTask                                 Init();
		UniTask<WalletProviderData>             Connect(WalletProviderData data);
		UniTask<bool>                           Disconnect();
		UniTask<OperationResponse>              RequestOperation(OperationRequest                   operationRequest);
		UniTask<SignPayloadResponse>            RequestSignPayload(SignPayloadRequest               signRequest);
		UniTask                                 RequestContractOrigination(OriginateContractRequest originationRequest);
		bool                                    IsAlreadyConnected();
	}
}