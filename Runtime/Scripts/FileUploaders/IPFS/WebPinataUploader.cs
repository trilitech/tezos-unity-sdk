namespace TezosSDK.FileUploaders.IPFS
{

#if UNITY_WEBGL
	public class WebPinataUploader : BaseUploader, IPinataUploader
	{
		#region IBaseUploader Implementation

		public IEnumerator UploadFile(Action<string> callback)
		{
			yield return null;
			WebPinataUploaderHelper.RequestFile(callback, SupportedFileExtensions);
		}

		#endregion

		#region IPinataUploader Implementation

		public PinataCredentials PinataCredentials { get; set; }

		#endregion

		public void FileRequestCallback(string path)
		{
			WebPinataUploaderHelper.SetResult(path);
		}
	}

#endif

}