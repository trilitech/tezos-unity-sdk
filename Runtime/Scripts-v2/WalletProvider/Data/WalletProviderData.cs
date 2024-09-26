namespace Tezos.WalletProvider
{
	public enum WalletType
	{
		BEACON,
		WALLETCONNECT
	}

	public class WalletProviderData
	{
		public WalletType WalletType    { get; set; }
		public string     WalletAddress { get; set; }
		public string     PublicKey     { get; set; }
		public string     Network       { get; set; }
		public string     PairingUri    { get; set; }
	}
}