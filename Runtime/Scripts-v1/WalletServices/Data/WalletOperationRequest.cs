namespace TezosSDK.WalletServices.Data
{

	public struct WalletOperationRequest
	{
		public string Destination;
		public string EntryPoint;
		public string Arg;
		public ulong Amount;
	}

}