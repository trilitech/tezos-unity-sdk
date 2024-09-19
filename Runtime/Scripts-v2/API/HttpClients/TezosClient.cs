using System;
using System.Text;
using Dynamic.Json;
using Newtonsoft.Json;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using UnityEngine.Networking;

namespace Tezos.API
{
	public class TezosClient
	{
		protected TezosClient(int timeOut) => RequestTimeout = timeOut;

		private int RequestTimeout { get; }

		private T DeserializeJson<T>(string json)
		{
			if (typeof(T) == typeof(string))
			{
				return (T)(object)DJson.Parse(json, JsonOptions.DefaultOptions);
			}

			return JsonConvert.DeserializeObject<T>(json);
		}

		private void HandleResponse<T>(UnityWebRequest request, Action<HttpResult<T>> callback, out T result)
		{
			result = default;
			if (request.result != UnityWebRequest.Result.Success)
			{
				TezosLogger.LogError($"Request failed with error: {request.error}");
				callback?.Invoke(new HttpResult<T>(request.error));
			}
			else
			{
				var downloadHandlerText = request.downloadHandler.text;
				try
				{
					TezosLogger.LogDebug($"HandleResponse<{typeof(T).Name}>: {downloadHandlerText}");
					result = DeserializeJson<T>(downloadHandlerText);
					callback?.Invoke(new HttpResult<T>(result));
				}
				catch (Exception ex)
				{
					TezosLogger.LogError($"Failed to deserialize JSON: {ex.Message}");
					callback?.Invoke(new HttpResult<T>(ex.Message));
				}
			}
		}

		public async UniTask<T> GetRequest<T>(string endpoint)
		{
			TezosLogger.LogDebug($"GET: {endpoint}");
			using UnityWebRequest request = UnityWebRequest.Get(endpoint);
			request.timeout = RequestTimeout;
			request.SetRequestHeader(HttpHeaders.Accept.Key,    HttpHeaders.Accept.Value);
			request.SetRequestHeader(HttpHeaders.UserAgent.Key, HttpHeaders.UserAgent.Value);
			var operation = request.SendWebRequest();
			while (!operation.isDone)
			{
				await UniTask.Yield();
			}

			if (request.result != UnityWebRequest.Result.Success)
			{
				TezosLogger.LogError($"GET request failed: {request.error}");
				throw new Exception($"GET request to {endpoint} failed: {request.error}");
			}

			string responseBody = request.downloadHandler.text;
			TezosLogger.LogDebug($"Response from GET: {responseBody}");
			return DeserializeJson<T>(responseBody);
		}

		public async UniTask<T> PostRequest<T>(string endpoint, object data)
		{
			TezosLogger.LogDebug($"POST: {endpoint}");
			string                jsonData = JsonConvert.SerializeObject(data);
			byte[]                bodyRaw  = Encoding.UTF8.GetBytes(jsonData);
			using UnityWebRequest request  = new UnityWebRequest(endpoint, "POST");
			request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.timeout         = RequestTimeout;
			request.SetRequestHeader(HttpHeaders.Accept.Key,      HttpHeaders.Accept.Value);
			request.SetRequestHeader(HttpHeaders.UserAgent.Key,   HttpHeaders.UserAgent.Value);
			request.SetRequestHeader(HttpHeaders.ContentType.Key, "application/json");
			var operation = request.SendWebRequest();
			while (!operation.isDone)
			{
				await UniTask.Yield();
			}

			if (request.result != UnityWebRequest.Result.Success)
			{
				TezosLogger.LogError($"POST request failed: {request.error}");
				throw new Exception($"POST request to {endpoint} failed: {request.error}");
			}

			string responseBody = request.downloadHandler.text;
			TezosLogger.LogDebug($"Response from POST: {responseBody}");
			return DeserializeJson<T>(responseBody);
		}
	}
}