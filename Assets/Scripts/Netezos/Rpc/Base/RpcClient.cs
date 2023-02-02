using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Dynamic.Json;
using Dynamic.Json.Extensions;
using UnityEngine;
using UnityEngine.Networking;

namespace Netezos.Rpc
{
    class RpcClient : IDisposable
    {
        #region static
        static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(2);

        static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            MaxDepth = 100_000,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            PropertyNamingPolicy = new SnakeCaseNamingPolicy()
        };
        #endregion

        Uri BaseAddress { get; }
        TimeSpan RequestTimeout { get; }
        DateTime Expiration;

        HttpClient _HttpClient;
        protected HttpClient HttpClient
        {
            get
            {
                lock (this)
                {
                    // Workaround for https://github.com/dotnet/runtime/issues/18348
                    if (DateTime.UtcNow > Expiration)
                    {
                        _HttpClient?.Dispose();
                        _HttpClient = new HttpClient();

                        _HttpClient.BaseAddress = BaseAddress;
                        _HttpClient.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        _HttpClient.DefaultRequestHeaders.UserAgent.Add(
                            new ProductInfoHeaderValue("Netezos", Version));
                        _HttpClient.Timeout = RequestTimeout;

                        Expiration = DateTime.UtcNow.AddMinutes(60);
                    }
                }

                return _HttpClient;
            }
        }

        public RpcClient(string baseUri, int timeoutSec = 30)
        {
            if (string.IsNullOrEmpty(baseUri))
                throw new ArgumentNullException(nameof(baseUri));

            if (!Uri.IsWellFormedUriString(baseUri, UriKind.Absolute))
                throw new ArgumentException("Invalid URI");

            BaseAddress = new Uri($"{baseUri.TrimEnd('/')}/");
            RequestTimeout = TimeSpan.FromSeconds(timeoutSec);
        }

        public RpcClient(HttpClient client)
        {
            _HttpClient = client ?? throw new ArgumentNullException(nameof(client));
            _HttpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("Netezos", Version));

            Expiration = DateTime.MaxValue;
        }

        /// <summary>
		/// Returns a dynamic json
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
        public IEnumerator GetJson(string path)
        {
            using (var newRequest = UnityWebRequest.Get(BaseAddress + path))
            {
                newRequest.SetRequestHeader("Accept", "application/json");
                newRequest.SetRequestHeader("Content-Type", "application/json");
                newRequest.SetRequestHeader("User-Agent", "Netezos/" + Assembly.GetExecutingAssembly().GetName().Version.ToString(2));
                newRequest.timeout = (int)10;
                newRequest.SendWebRequest();

                yield return new WaitWhile(() => !newRequest.isDone);
                
                var jsonText = newRequest.downloadHandler.text;

                var djson = DJson.Parse(jsonText, DefaultOptions);

                yield return djson;
                //yield return JsonSerializer.Deserialize<dynamic>(jsonText, DefaultOptions);
            }
        }

        public IEnumerator GetJson<T>(string path)
        {
            using (var newRequest = UnityWebRequest.Get(BaseAddress + path))
            {
                Debug.Log("GET path: " + BaseAddress + path);

                newRequest.SetRequestHeader("Accept", "application/json");
                newRequest.SetRequestHeader("Content-Type", "application/json");
                newRequest.SetRequestHeader("User-Agent", "Netezos/" + Assembly.GetExecutingAssembly().GetName().Version.ToString(2));
                newRequest.timeout = (int)10;
                newRequest.SendWebRequest();

                yield return new WaitWhile(() => !newRequest.isDone);

                yield return JsonSerializer.Deserialize<T>(newRequest.downloadHandler.text, DefaultOptions);
            }
        }

        /// <summary>
		/// Returns dynamyc type
		/// </summary>
		/// <param name="path"></param>
		/// <param name="data"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
        public IEnumerator PostJson(string path, object data)
        {
            var content = JsonSerializer.Serialize(data, DefaultOptions);
            yield return PostJson(path, content);
        }

        /// <summary>
		/// Returns dynamyc json type
		/// </summary>
		/// <param name="path"></param>
		/// <param name="content"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
        public IEnumerator PostJson(string path, string content)
        {
            using (var newRequest = new UnityWebRequest(BaseAddress + path, "POST"))
            {
                newRequest.SetRequestHeader("Accept", "application/json");
                newRequest.SetRequestHeader("Content-Type", "application/json");
                newRequest.SetRequestHeader("User-Agent", "Netezos/" + Assembly.GetExecutingAssembly().GetName().Version.ToString(2));

                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(content);
                newRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                newRequest.downloadHandler = new DownloadHandlerBuffer();

                newRequest.timeout = (int)10;
                newRequest.SendWebRequest();

                yield return new WaitWhile(() => !newRequest.isDone);

                Debug.Log("POST: " + BaseAddress + path);
                yield return EnsureResponseSuccessfull(newRequest.result, newRequest.responseCode, newRequest.error + " | " + newRequest.downloadHandler.text);

                yield return DJson.Parse(newRequest.downloadHandler.text, DefaultOptions);
                //yield return JsonSerializer.Deserialize<T>(newRequest.downloadHandler.text, DefaultOptions);
            }
        }

        public IEnumerator PostJson<T>(string path, object data) => PostJson<T>(path, 
            //data.ToString()
            JsonSerializer.Serialize(data, 
            DefaultOptions
            //new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
            )/*.Replace("\u0022", "\"")*/ // replace unicode double quotes due to serialization with actual quotes
        );

        public IEnumerator PostJson<T>(string path, string content)
        {
            using (var newRequest = new UnityWebRequest(BaseAddress + path, "POST"))
            {
                newRequest.SetRequestHeader("Accept", "application/json");
                newRequest.SetRequestHeader("Content-Type", "application/json");
                newRequest.SetRequestHeader("User-Agent", "Netezos/" + Assembly.GetExecutingAssembly().GetName().Version.ToString(2));
    
                Debug.Log("POST path: " + BaseAddress + path);
                Debug.Log("content: " + content);

                var jsonToSend = new System.Text.UTF8Encoding().GetBytes(content);
                newRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                newRequest.downloadHandler = new DownloadHandlerBuffer();

                newRequest.timeout = (int)10;
                newRequest.SendWebRequest();

                yield return new WaitWhile(() => !newRequest.isDone);

                Debug.Log("POST: " + BaseAddress + path);
                Debug.Log("content: " + content);
                yield return EnsureResponseSuccessfull(newRequest.result, newRequest.responseCode, newRequest.error + " | " + newRequest.downloadHandler.text);

                if (newRequest.result != UnityWebRequest.Result.Success)
                    yield return default(T);

                else
                {                
                    var textResult = newRequest.downloadHandler.text;
                    var jsonResult = JsonSerializer.Deserialize<T>(textResult, DefaultOptions);
                    Debug.Log("Result: " + jsonResult);
                    yield return jsonResult;
                }
            }
        }

        public void Dispose()
        {
            _HttpClient?.Dispose();
        }

        private IEnumerator EnsureResponseSuccessfull(UnityWebRequest.Result response, long responseCode, string errorMessage)
        {
            if (response != UnityWebRequest.Result.Success)
            {
                /*
                switch (responseCode)
                {
                    case 400:
                        throw new BadRequestException(errorMessage);
                    case 500:
                        throw new InternalErrorException(errorMessage);
                    default:
                        throw new RpcException((HttpStatusCode)responseCode, errorMessage);
                }
                */
                Debug.LogError(errorMessage);
            }
            yield return null;
        }
    }
}
