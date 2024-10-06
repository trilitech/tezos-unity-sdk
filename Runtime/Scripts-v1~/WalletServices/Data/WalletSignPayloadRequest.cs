using Beacon.Sdk.Beacon.Sign;

namespace TezosSDK.WalletServices.Data
{

	public struct WalletSignPayloadRequest
	{
		public SignPayloadType SigningType;
		public string Payload;
	}

}