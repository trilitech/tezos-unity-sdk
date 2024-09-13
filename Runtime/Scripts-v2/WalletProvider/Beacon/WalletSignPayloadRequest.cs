using Beacon.Sdk.Beacon.Sign;

namespace TezosSDK.WalletProvider
{

	public struct WalletSignPayloadRequest
	{
		public SignPayloadType SigningType;
		public string Payload;
	}

}