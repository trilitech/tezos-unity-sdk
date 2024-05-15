using TezosSDK.FileUploaders.IPFS;

namespace TezosSDK.FileUploaders.Interfaces
{

	public interface IPinataUploader : IBaseUploader
	{
		PinataCredentials PinataCredentials { get; set; }
	}

}