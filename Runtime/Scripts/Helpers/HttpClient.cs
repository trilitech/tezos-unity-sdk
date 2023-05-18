using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Dynamic.Json;
using Helpers;
using Scripts.Tezos;
using UnityEngine;
using UnityEngine.Networking;

namespace Scripts.Helpers
{
    public class HttpClient
    {
        private string BaseAddress { get; }
        private int RequestTimeout { get; }

        protected HttpClient(string baseAddress)
        {
            BaseAddress = baseAddress.EndsWith("/") ? baseAddress : $"{baseAddress}/";
            RequestTimeout = TezosConfig.Instance.DefaultTimeoutSeconds;
        }
        
        protected HttpClient(IDataProviderConfig config)
        {
            var configBaseAddress = config.BaseUrl;
            BaseAddress = configBaseAddress.EndsWith("/") ? configBaseAddress : $"{configBaseAddress}/";
            RequestTimeout = config.TimeoutSeconds;
        }

        protected IEnumerator GetJson(string path)
        {
            var request = GetUnityWebRequest(UnityWebRequest.kHttpVerbGET, path);
            request.SendWebRequest();
            yield return new WaitUntil(() => request.isDone);
            yield return DJson.Parse(request.downloadHandler.text, JsonOptions.DefaultOptions);
            request.Dispose();
        }

        protected IEnumerator GetJson<T>(string path)
        {
            var request = GetUnityWebRequest(UnityWebRequest.kHttpVerbGET, path);
            request.SendWebRequest();
            yield return new WaitUntil(() => request.isDone);
            yield return JsonSerializer.Deserialize<T>(request.downloadHandler.text, JsonOptions.DefaultOptions);
            request.Dispose();
        }

        protected IEnumerator PostJson<T>(string path, object data)
        {
            var serializedData = JsonSerializer.Serialize(data, JsonOptions.DefaultOptions);
            var request = GetUnityWebRequest(UnityWebRequest.kHttpVerbPOST, path);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(serializedData));
            request.SetRequestHeader(HttpHeaders.ContentType.Key, HttpHeaders.ContentType.Value);
            request.SendWebRequest();
            yield return new WaitUntil(() => request.isDone);
            yield return JsonSerializer.Deserialize<T>(request.downloadHandler.text, JsonOptions.DefaultOptions);
            request.Dispose();
        }

        private UnityWebRequest GetUnityWebRequest(string method, string path)
        {
            var request = new UnityWebRequest($"{BaseAddress}{path}", method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(HttpHeaders.Accept.Key, HttpHeaders.Accept.Value);
            request.SetRequestHeader(HttpHeaders.UserAgent.Key, HttpHeaders.UserAgent.Value);
            request.timeout = RequestTimeout;
            return request;
        }
    }

    internal static class HttpHeaders
    {
        public static KeyValuePair<string, string> ContentType => new("Content-Type", "application/json");
        public static KeyValuePair<string, string> Accept => new("Accept", "application/json");
        public static KeyValuePair<string, string> UserAgent => new("User-Agent", "tezos-unity-sdk");
    }
}