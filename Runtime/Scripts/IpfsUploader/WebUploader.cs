using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TezosSDK.Scripts.IpfsUploader
{
    public class WebUploader : BaseUploader, IFileUploader
    {
        public void FileRequestCallback(string path)
        {
            WebUploaderHelper.SetResult(path);
        }

        public IEnumerator UploadFile(Action<string> callback)
        {
            yield return null;
            WebUploaderHelper.RequestFile(callback, SupportedFileExtensions);
        }
    }
    
    public static class WebUploaderHelper
    {
        private static Action<string> _responseCallback;

        public static WebUploader InitWebFileLoader()
        {
            const string callbackObjectName = nameof(WebUploader);
            const string callbackMethodName = nameof(WebUploader.FileRequestCallback);

            var webUploaderGameObject = GameObject.Find(nameof(WebUploader));
            
            var webFileUploader = webUploaderGameObject != null
                ? webUploaderGameObject.GetComponent<WebUploader>()
                : new GameObject(nameof(WebUploader)).AddComponent<WebUploader>();
            
            JsInitFileLoader(
                callbackObjectName,
                callbackMethodName,
                webFileUploader.ApiUrl,
                webFileUploader.ApiKey);
            
            return webFileUploader;
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
        private static extern void JsInitFileLoader(
            string objectName,
            string methodName,
            string apiUrl,
            string apiKey);

        [DllImport("__Internal")]
        private static extern void JsRequestUserFile(string extensions);
    }
}