namespace TezosSDK.FileUploaders
{

	public interface IPinataUploader : IBaseUploader
	{
		PinataCredentials PinataCredentials { get; set; }
	}

}