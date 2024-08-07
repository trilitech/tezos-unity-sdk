using System;
using System.Threading.Tasks;
using TezosSDK.Tezos.Models;
using TezosSDK.WalletServices.Connectors;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using TezosSDK.WalletServices.Interfaces;

namespace TezosSDK.Tezos.Interfaces.Wallet
{

	public interface IWalletConnector : IDisposable
	{
		ConnectorType                   ConnectorType      { get; }
		PairingRequestData              PairingRequestData { get; }
		event Action<WalletMessageType> OperationRequested;
		void                            ConnectWallet();
		string                          GetWalletAddress();
		void                            DisconnectWallet();
		void                            RequestOperation(WalletOperationRequest                   operationRequest);
		void                            RequestSignPayload(WalletSignPayloadRequest               signRequest);
		void                            RequestContractOrigination(WalletOriginateContractRequest originationRequest);
		Task                            InitializeAsync(IWalletEventManager                       walletEventManager);
	}
}