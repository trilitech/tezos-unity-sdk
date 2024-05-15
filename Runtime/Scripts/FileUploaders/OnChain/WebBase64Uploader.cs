using System;
using System.Collections;
using TezosSDK.FileUploaders.Interfaces;

namespace TezosSDK.FileUploaders.OnChain
{

#if UNITY_WEBGL
	public class WebBase64Uploader : BaseUploader, IBaseUploader
	{
		#region IBaseUploader Implementation

		public IEnumerator UploadFile(Action<string> callback)
		{
			yield return null;
			WebBase64UploaderHelper.RequestFile(callback, SupportedFileExtensions);
		}

		#endregion

		public void FileRequestCallback(string path)
		{
			WebBase64UploaderHelper.SetResult(path);
		}
	}

#endif

}