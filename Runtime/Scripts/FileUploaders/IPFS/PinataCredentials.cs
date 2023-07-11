namespace TezosSDK.Scripts.FileUploaders.IPFS
{
    public class PinataCredentials
    {
        public string ApiUrl { get; }
        public string ApiKey { get; }

        public PinataCredentials(string apiKey)
        {
            ApiUrl = "https://api.pinata.cloud/pinning/pinFileToIPFS";
            ApiKey = apiKey;
        }
    }
}