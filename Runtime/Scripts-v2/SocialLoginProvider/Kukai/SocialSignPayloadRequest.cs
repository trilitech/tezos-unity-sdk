using Beacon.Sdk.Beacon.Sign;

namespace TezosSDK.SocialLoginProvider
{
	public struct SocialSignPayloadRequest
	{
		public SignPayloadType SigningType;
		public string Payload;
	}
}