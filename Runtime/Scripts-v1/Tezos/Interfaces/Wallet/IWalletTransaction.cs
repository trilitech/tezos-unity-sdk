using Beacon.Sdk.Beacon.Sign;

namespace TezosSDK.Tezos.Interfaces.Wallet
{

	public interface IWalletTransaction
	{
		void RequestSignPayload(SignPayloadType signingType, string payload);
		bool VerifySignedPayload(SignPayloadType signingType, string payload);
		void CallContract(string contractAddress, string entryPoint, string input, ulong amount = 0);
	}

}