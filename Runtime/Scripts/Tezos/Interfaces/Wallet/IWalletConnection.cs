using TezosSDK.Tezos.Models;
using TezosSDK.WalletServices.Connectors;
using TezosSDK.WalletServices.Data;


namespace TezosSDK.Tezos.Interfaces.Wallet
{
	public interface IWalletConnection
	{
		ConnectorType      ConnectorType      { get; }
		bool               IsConnected        { get; }
		PairingRequestData PairingRequestData { get; }
		string             GetWalletAddress();
		void               Connect(ConnectorType connectorType);
		void               Disconnect();
		void               RequestOperation(WalletOperationRequest                   operationRequest);
		void               RequestSignPayload(WalletSignPayloadRequest               signRequest);
		void               RequestContractOrigination(WalletOriginateContractRequest originationRequest);
	}
}