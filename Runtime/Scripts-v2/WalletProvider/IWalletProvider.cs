using System;
using Beacon.Sdk.Beacon.Operation;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.MessageSystem;

namespace Tezos.WalletProvider
{
	public interface IWalletProvider
	{
		public event Action<string> PairingRequested;
		public WalletType           WalletType { get; }
		UniTask                     Init(IContext              context);
		UniTask<WalletProviderData> Connect(WalletProviderData data);
		UniTask<bool>               Disconnect();
		UniTask<OperationResponse>  RequestOperation(WalletOperationRequest                   operationRequest);
		UniTask<WalletProviderData> RequestSignPayload(WalletSignPayloadRequest               signRequest);
		UniTask                     RequestContractOrigination(WalletOriginateContractRequest originationRequest);
		bool                        IsAlreadyConnected();
	}
}