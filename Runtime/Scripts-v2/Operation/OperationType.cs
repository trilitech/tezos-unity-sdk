namespace Tezos.Operation
{
	public enum OperationType
	{
		PERMISSION_REQUEST,
		SIGN_PAYLOAD_REQUEST,
		OPERATION_REQUEST,
		BROADCAST_REQUEST,
		PERMISSION_RESPONSE,
		SIGN_PAYLOAD_RESPONSE,
		OPERATION_RESPONSE,
		BROADCAST_RESPONSE,
		ACKNOWLEDGE,
		DISCONNECT,
		ERROR,
	}
}