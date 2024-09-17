using Beacon.Sdk.Beacon.Sign;

namespace Tezos.WalletProvider
{

	public struct WalletSignPayloadRequest
	{
		public SignPayloadType SigningType;
		public string Payload;
	}

}