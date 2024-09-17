using Beacon.Sdk.Beacon.Sign;

namespace Tezos.SocialLoginProvider
{
	public struct SocialSignPayloadRequest
	{
		public SignPayloadType SigningType;
		public string Payload;
	}
}