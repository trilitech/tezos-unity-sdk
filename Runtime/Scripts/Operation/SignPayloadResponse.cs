namespace Tezos.Operation
{
	public struct SignPayloadResponse
	{
		public string          Id;
		public string          SenderId;
		public string          Signature;
		public SignPayloadType Type;
	}
}