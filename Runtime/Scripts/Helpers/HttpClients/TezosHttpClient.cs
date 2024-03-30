using System;
using System.Collections;
using System.Text;
using System.Text.Json;
using Dynamic.Json;
using TezosSDK.Tezos;
using UnityEngine.Networking;

namespace TezosSDK.Helpers.HttpClients
{

	public class Result<T>
	{
		public Result(T data)
		{
			Data = data;
			Success = true;
			ErrorMessage = string.Empty;
		}

		public Result(string errorMessage)
		{
			Data = default;
			Success = false;
			ErrorMessage = errorMessage;
		}

		public T Data { get; private set; }
		public bool Success { get; private set; }
		public string ErrorMessage { get; private set; }
	}

	public class TezosHttpClient
	{
		protected TezosHttpClient(TezosConfigSO rpcConfig)
		{
			BaseAddress = AddSlashIfNeeded(rpcConfig.Rpc);
			RequestTimeout = rpcConfig.RequestTimeoutSeconds;
		}

		protected TezosHttpClient(DataProviderConfigSO dataProviderConfig)
		{
			BaseAddress = AddSlashIfNeeded(dataProviderConfig.BaseUrl);
			RequestTimeout = dataProviderConfig.RequestTimeoutSeconds;
		}

		private string BaseAddress { get; }
		private int RequestTimeout { get; }

		private T DeserializeJson<T>(string json)
		{
			if (typeof(T) == typeof(string))
			{
				return (T)(object)DJson.Parse(json, JsonOptions.DefaultOptions);
			}

			return JsonSerializer.Deserialize<T>(json, JsonOptions.DefaultOptions);
		}

		private string AddSlashIfNeeded(string url)
		{
			return url.EndsWith("/") ? url : $"{url}/";
		}

		private void HandleResponse<T>(UnityWebRequest request, Action<Result<T>> callback, out T result)
		{
			result = default;

			if (request.result != UnityWebRequest.Result.Success)
			{
				Logger.LogError($"Request failed with error: {request.error}");
				callback?.Invoke(new Result<T>(request.error));
			}
			else
			{
				var downloadHandlerText = request.downloadHandler.text;

				if (string.IsNullOrWhiteSpace(downloadHandlerText))
				{
					var msg =
						$"Request: {request.url} - Status code: {request.responseCode} - Error: {request.error} - Response: {downloadHandlerText}";

					Logger.LogError(msg);
					callback?.Invoke(new Result<T>(msg));
				}
				else
				{
					try
					{
						result = DeserializeJson<T>(downloadHandlerText);
						callback?.Invoke(new Result<T>(result));
					}
					catch (Exception ex)
					{
						callback?.Invoke(new Result<T>(ex.Message));
					}
				}
			}
		}

		protected IEnumerator GetJsonCoroutine<T>(string path, Action<Result<T>> callback = null)
		{
			using var request = GetUnityWebRequest(UnityWebRequest.kHttpVerbGET, path);
			yield return request.SendWebRequest();

			HandleResponse(request, callback, out var result);
			yield return result;
		}

		protected IEnumerator PostJsonCoroutine<T>(string path, object data, Action<Result<T>> callback)
		{
			var serializedData = JsonSerializer.Serialize(data, JsonOptions.DefaultOptions);
			using var request = GetUnityWebRequest(UnityWebRequest.kHttpVerbPOST, path);
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(serializedData));
			request.SetRequestHeader(HttpHeaders.ContentType.Key, HttpHeaders.ContentType.Value);
			yield return request.SendWebRequest();

			HandleResponse(request, callback, out var result);
			yield return result;
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

}