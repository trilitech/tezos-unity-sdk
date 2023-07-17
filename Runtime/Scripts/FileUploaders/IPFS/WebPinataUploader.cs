using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.Json;
using TezosSDK.Scripts.IpfsUploader;
using UnityEngine;

namespace TezosSDK.Scripts.FileUploaders.IPFS
{
#if UNITY_WEBGL && !UNITY_EDITOR
    public class WebPinataUploader : BaseUploader, IPinataUploader
    {
        public PinataCredentials PinataCredentials { get; set; }
        
        public void FileRequestCallback(string path)
        {
            WebPinataUploaderHelper.SetResult(path);
        }

        public IEnumerator UploadFile(Action<string> callback)
        {
            yield return null;
            WebPinataUploaderHelper.RequestFile(callback, SupportedFileExtensions);
        }
    }

    public static class WebPinataUploaderHelper
    {
        private static Action<string> _responseCallback;

        public static IPinataUploader GetUploader(string apiKey)
        {
            const string callbackObjectName = nameof(WebPinataUploader);
            const string callbackMethodName = nameof(WebPinataUploader.FileRequestCallback);

            var webUploaderGameObject = GameObject.Find(nameof(WebPinataUploader));
            var webFileUploader = webUploaderGameObject != null
                ? webUploaderGameObject.GetComponent<WebPinataUploader>()
                : new GameObject(nameof(WebPinataUploader)).AddComponent<WebPinataUploader>();
            
            webFileUploader.PinataCredentials = new PinataCredentials(apiKey);

            JsInitPinataUploader(
                callbackObjectName,
                callbackMethodName,
                webFileUploader.PinataCredentials.ApiUrl,
                webFileUploader.PinataCredentials.ApiKey);

            return webFileUploader;
        }

        public static void RequestFile(Action<string> callback, string extensions)
        {
            JsRequestUserFile(extensions);
            _responseCallback = callback;
        }

        public static void SetResult(string response)
        {
            var ipfsResponse = JsonSerializer.Deserialize<IpfsResponse>(response);
            _responseCallback.Invoke($"ipfs://{ipfsResponse.IpfsHash}");
            Dispose();
        }

        private static void Dispose()
        {
            _responseCallback = null;
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
