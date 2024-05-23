using System;
using System.Runtime.InteropServices;
using TezosSDK.FileUploaders.Interfaces;
using UnityEngine;

namespace TezosSDK.FileUploaders.OnChain
{
#if UNITY_WEBGL

	public static class WebBase64UploaderHelper
	{
		private static Action<string> responseCallback;

		public static IBaseUploader GetUploader()
		{
			const string _callback_object_name = nameof(WebBase64Uploader);
			const string _callback_method_name = nameof(WebBase64Uploader.FileRequestCallback);

			var webUploaderGameObject = GameObject.Find(nameof(WebBase64Uploader));

			var uploader = webUploaderGameObject != null
				? webUploaderGameObject.GetComponent<WebBase64Uploader>()
				: new GameObject(nameof(WebBase64Uploader)).AddComponent<WebBase64Uploader>();

			JsInitBase64Uploader(_callback_object_name, _callback_method_name);

			return uploader;
		}

		public static void RequestFile(Action<string> callback, string extensions)
		{
			JsRequestUserFile(extensions);
			responseCallback = callback;
		}

		public static void SetResult(string response)
		{
			responseCallback.Invoke(response);
			Dispose();
		}

		private static void Dispose()
		{
			responseCallback = null;
		}

		[DllImport("__Internal")]
		private static extern void JsInitBase64Uploader(string objectName, string methodName);

		[DllImport("__Internal")]
		private static extern void JsRequestUserFile(string extensions);
	}
#endif
}