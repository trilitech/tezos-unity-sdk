// ReSharper disable once RedundantUsingDirective

using System.Collections;
// ReSharper disable once RedundantUsingDirective
using System;
// ReSharper disable once RedundantUsingDirective
using TezosSDK.FileUploaders;

// ReSharper disable once EmptyNamespace
namespace TezosSDK.FileUploaders
{

#if UNITY_WEBGL && !UNITY_EDITOR
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