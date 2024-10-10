namespace Tezos.Operation
{
	public struct SignPayloadRequest
	{
		public SignPayloadType SigningType;
		public string          Payload;
	}
}