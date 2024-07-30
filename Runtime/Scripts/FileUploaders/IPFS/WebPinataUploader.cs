// ReSharper disable once RedundantUsingDirective
using System;
// ReSharper disable once RedundantUsingDirective
using System.Collections;
// ReSharper disable once RedundantUsingDirective
using TezosSDK.FileUploaders.Interfaces;

// ReSharper disable once EmptyNamespace
namespace TezosSDK.FileUploaders.IPFS
{

#if UNITY_WEBGL && !UNITY_EDITOR
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