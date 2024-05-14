using System;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Tezos;

namespace TezosSDK.Beacon
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
	
	
	public enum WalletMessageType
	{
		ConnectionRequest,
		OperationRequest,
		SignPayloadRequest,
		DisconnectionRequest
	}
	
	public struct WalletOperationRequest
	{
		public string Destination;
		public string EntryPoint;
		public string Arg;
		public ulong Amount;
	}
	
	public struct WalletSignPayloadRequest
	{
		public SignPayloadType SigningType;
		public string Payload;
	}
	
	public struct WalletOriginateContractRequest
	{
		public string Script;
		public string DelegateAddress;
	}
}