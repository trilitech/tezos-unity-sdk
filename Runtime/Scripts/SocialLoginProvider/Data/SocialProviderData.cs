namespace Tezos.SocialLoginProvider
{
	public enum SocialLoginType
	{
		Kukai
	}

	public class SocialProviderData
	{
		public SocialLoginType SocialLoginType { get; set; }
		public string          WalletAddress   { get; set; }
		public string          PublicKey       { get; set; }
		public string          LoginDetails    { get; set; }
		public TypeOfLogin     LoginType       { get; set; }
	}
}