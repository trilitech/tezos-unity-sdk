using Beacon.Sdk.Beacon.Sign;

namespace TezosSDK.Tezos.Wallet
{
	
	public interface IWalletEventProvider
	{
		IWalletEventManager EventManager { get; }
	}
	
	public interface IWalletConnection
	{
		bool IsConnected { get; }
		HandshakeData HandshakeData { get; }

		void Connect(WalletProviderType walletProvider);
		void Disconnect();
	}

	public interface IWalletAccount
	{
		string GetWalletAddress();
	}

	public interface IWalletTransaction
	{
		void RequestSignPayload(SignPayloadType signingType, string payload);
		bool VerifySignedPayload(SignPayloadType signingType, string payload);
		void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0);
	}

	public interface IWalletContract
	{
		void OriginateContract(string script, string delegateAddress = null);
	}

}