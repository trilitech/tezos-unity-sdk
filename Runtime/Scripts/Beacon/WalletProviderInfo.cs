#region

using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;

#endregion

namespace TezosSDK.Beacon
{

	public class WalletProviderInfo
	{
		public string Network { get; set; }
		public string Rpc { get; set; }
		public DAppMetadata Metadata { get; set; }
	}

}