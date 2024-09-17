using System;
using System.Threading.Tasks;
using Tezos.MessageSystem;

namespace Tezos.WalletProvider
{
	public interface IWalletProvider
	{
		public WalletType WalletType { get; }
		Task Init(IContext context);
		Task<WalletProviderData> Connect(WalletProviderData data);
		Task<bool> Disconnect();
		Task RequestOperation(WalletOperationRequest operationRequest);
		Task RequestSignPayload(WalletSignPayloadRequest signRequest);
		Task RequestContractOrigination(WalletOriginateContractRequest originationRequest);
		bool IsAlreadyConnected();
	}
}
