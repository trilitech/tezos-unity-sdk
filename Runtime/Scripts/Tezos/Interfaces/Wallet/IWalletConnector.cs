using System;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;

namespace TezosSDK.Tezos.Interfaces.Wallet
{

	public interface IWalletConnector
	{
		event Action<WalletMessageType> OperationRequested;
		void ConnectWallet();
		string GetWalletAddress();
		void DisconnectWallet();
		void RequestOperation(WalletOperationRequest operationRequest);
		void RequestSignPayload(WalletSignPayloadRequest signRequest);
		void RequestContractOrigination(WalletOriginateContractRequest originationRequest);
	}

}