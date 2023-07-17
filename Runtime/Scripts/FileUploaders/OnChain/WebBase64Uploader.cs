using System;
using System.Collections;
using System.Runtime.InteropServices;
using Logger = TezosSDK.Helpers.Logger;
using UnityEngine;

namespace TezosSDK.Scripts.FileUploaders.OnChain
{
#if UNITY_WEBGL && !UNITY_EDITOR
    public class WebBase64Uploader : BaseUploader, IBaseUploader
    {
        public void FileRequestCallback(string path)
        {
            WebBase64UploaderHelper.SetResult(path);
        }

        public IEnumerator UploadFile(Action<string> callback)
        {
            yield return null;
            WebBase64UploaderHelper.RequestFile(callback, SupportedFileExtensions);
        }
    }

    public static class WebBase64UploaderHelper
    {
        private static Action<string> _responseCallback;

        public static IBaseUploader GetUploader()
        {
            const string callbackObjectName = nameof(WebBase64Uploader);
            const string callbackMethodName = nameof(WebBase64Uploader.FileRequestCallback);

            var webUploaderGameObject = GameObject.Find(nameof(WebBase64Uploader));
            var uploader = webUploaderGameObject != null
                ? webUploaderGameObject.GetComponent<WebBase64Uploader>()
                : new GameObject(nameof(WebBase64Uploader)).AddComponent<WebBase64Uploader>();

            JsInitBase64Uploader(
                callbackObjectName,
                callbackMethodName);

            return uploader;
        }

        public static void RequestFile(Action<string> callback, string extensions)
        {
            JsRequestUserFile(extensions);
            _responseCallback = callback;
        }

        public static void SetResult(string response)
        {
            _responseCallback.Invoke(response);
            Dispose();
        }

        private static void Dispose()
        {
            _responseCallback = null;
        }

        [DllImport("__Internal")]
        private static extern void JsInitBase64Uploader(
            string objectName,
            string methodName);

        [DllImport("__Internal")]
        private static extern void JsRequestUserFile(string extensions);
    }
#endif
}
