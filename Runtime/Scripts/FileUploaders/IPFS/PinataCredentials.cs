namespace TezosSDK.FileUploaders.IPFS
{

	public class PinataCredentials
	{
		public PinataCredentials(string apiKey)
		{
			ApiUrl = "https://api.pinata.cloud/pinning/pinFileToIPFS";
			ApiKey = apiKey;
		}

		public string ApiUrl { get; }
		public string ApiKey { get; }
	}

}