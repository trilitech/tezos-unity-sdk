using TezosSDK.FileUploaders.IPFS;
using TezosSDK.FileUploaders.OnChain;
using UnityEngine;

namespace TezosSDK.FileUploaders
{

	public static class UploaderFactory
	{
		/// <summary>
		///     Cross-platform image uploader to IPFS network via Pinata service.
		/// </summary>
		/// <param name="apiKey">API key from https://app.pinata.cloud/developers/api-keys</param>
		public static IBaseUploader GetPinataUploader(string apiKey)
		{
			IPinataUploader uploader = null;

#if UNITY_WEBGL && !UNITY_EDITOR
			uploader = WebPinataUploaderHelper.GetUploader(apiKey);
#elif UNITY_EDITOR
			var editorUploaderGameObject = GameObject.Find(nameof(EditorPinataUploader));

			uploader = editorUploaderGameObject != null
				? editorUploaderGameObject.GetComponent<EditorPinataUploader>()
				: new GameObject(nameof(EditorPinataUploader)).AddComponent<EditorPinataUploader>();

			uploader.PinataCredentials = new PinataCredentials(apiKey);
#endif
			return uploader;
		}

		public static IBaseUploader GetOnchainUploader()
		{
			IBaseUploader uploader = null;

#if UNITY_WEBGL && !UNITY_EDITOR
			uploader = WebBase64UploaderHelper.GetUploader();
#elif UNITY_EDITOR
			var editorUploaderGameObject = GameObject.Find(nameof(EditorBase64Uploader));

			uploader = editorUploaderGameObject != null
				? editorUploaderGameObject.GetComponent<EditorBase64Uploader>()
				: new GameObject(nameof(EditorBase64Uploader)).AddComponent<EditorBase64Uploader>();
#endif
			return uploader;
		}
	}

}