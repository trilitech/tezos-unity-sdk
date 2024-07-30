// ReSharper disable once RedundantUsingDirective

using System;
// ReSharper disable once RedundantUsingDirective
using System.Runtime.InteropServices;
// ReSharper disable once RedundantUsingDirective
using System.Text.Json;
// ReSharper disable once RedundantUsingDirective
using TezosSDK.FileUploaders.Interfaces;
// ReSharper disable once RedundantUsingDirective
using UnityEngine;

// ReSharper disable once EmptyNamespace
namespace TezosSDK.FileUploaders.IPFS
{

#if UNITY_WEBGL && !UNITY_EDITOR
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

			JsInitPinataUploader(_callback_object_name, _callback_method_name, webFileUploader.PinataCredentials.ApiUrl, webFileUploader.PinataCredentials.ApiKey);

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
		private static extern void JsInitPinataUploader(string objectName, string methodName, string apiUrl, string apiKey);

		[DllImport("__Internal")]
		private static extern void JsRequestUserFile(string extensions);
	}
#endif

}