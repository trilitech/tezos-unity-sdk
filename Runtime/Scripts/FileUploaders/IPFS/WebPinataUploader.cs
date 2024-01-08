#if UNITY_WEBGL
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.Json;
using UnityEngine;
#endif

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

	public static class WebPinataUploaderHelper
	{
		private static Action<string> responseCallback;

		public static IPinataUploader GetUploader(string apiKey)
		{
			const string _callback_object_name = nameof(WebPinataUploader);
			const string _callback_method_name = nameof(WebPinataUploader.FileRequestCallback);

			var webUploaderGameObject = GameObject.Find(nameof(WebPinataUploader));

			var webFileUploader = webUploaderGameObject != null
				? webUploaderGameObject.GetComponent<WebPinataUploader>()
				: new GameObject(nameof(WebPinataUploader)).AddComponent<WebPinataUploader>();

			webFileUploader.PinataCredentials = new PinataCredentials(apiKey);

			JsInitPinataUploader(_callback_object_name, _callback_method_name, webFileUploader.PinataCredentials.ApiUrl,
				webFileUploader.PinataCredentials.ApiKey);

			return webFileUploader;
		}

		public static void RequestFile(Action<string> callback, string extensions)
		{
			JsRequestUserFile(extensions);
			responseCallback = callback;
		}

		public static void SetResult(string response)
		{
			var ipfsResponse = JsonSerializer.Deserialize<IpfsResponse>(response);
			responseCallback.Invoke($"ipfs://{ipfsResponse.IpfsHash}");
			Dispose();
		}

		private static void Dispose()
		{
			responseCallback = null;
		}

		[DllImport("__Internal")]
		private static extern void JsInitPinataUploader(
			string objectName,
			string methodName,
			string apiUrl,
			string apiKey);

		[DllImport("__Internal")]
		private static extern void JsRequestUserFile(string extensions);
	}
#endif

}