namespace Tezos.Operation
{
	public struct OperationResponse
	{
		public string        TransactionHash;
		public string        Id;
		public string        SenderId;
		public OperationType Type;
	}
}