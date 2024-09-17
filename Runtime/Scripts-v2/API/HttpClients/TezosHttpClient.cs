using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dynamic.Json;
using Newtonsoft.Json;
using Tezos.API;
using Tezos.Logger;
using UnityEngine.Networking;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tezos.Helpers.HttpClients
{

	public class TezosHttpClient
	{
		protected TezosHttpClient(string rpc, int timeOut)
		{
			BaseAddress = rpc;
			RequestTimeout = timeOut;
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

		// private string AddSlashIfNeeded(string url)
		// {
		// 	return url.EndsWith("/") ? url : $"{url}/";
		// }

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

		protected IEnumerator GetJsonCoroutine<T>(string path, Action<HttpResult<T>> callback = null)
		{
			TezosLogger.LogDebug($"GET: {BaseAddress}{path}");
			using var request = GetUnityWebRequest(UnityWebRequest.kHttpVerbGET, path);
			yield return request.SendWebRequest();

			HandleResponse(request, callback, out var result);
			yield return result;
		}

		protected IEnumerator PostJsonCoroutine<T>(string path, object data, Action<HttpResult<T>> callback)
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
			var request = new UnityWebRequest(Path.Combine(BaseAddress, path), method);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader(HttpHeaders.Accept.Key, HttpHeaders.Accept.Value);
			request.SetRequestHeader(HttpHeaders.UserAgent.Key, HttpHeaders.UserAgent.Value);
			request.timeout = RequestTimeout;
			return request;
		}

		public async Task<T> GetRequest<T>(string path)
		{
			string endpoint = Path.Combine(BaseAddress, path);
			TezosLogger.LogDebug($"GET: {endpoint}");
			
			using HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add(HttpHeaders.Accept.Key, HttpHeaders.Accept.Value);
			client.DefaultRequestHeaders.Add(HttpHeaders.UserAgent.Key, HttpHeaders.UserAgent.Value);

			HttpResponseMessage response = await client.GetAsync(endpoint);

			// Ensure we received a successful response
			response.EnsureSuccessStatusCode();

			// Parse the response as JSON and extract the balance
			string responseBody = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<T>(responseBody);
		}

		public IEnumerator PostRequest<T>(string path, object data, Action<HttpResult<T>> callback)
		{
			var serializedData = JsonConvert.SerializeObject(data);
			using var request = GetUnityWebRequest(UnityWebRequest.kHttpVerbPOST, path);
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(serializedData));
			request.SetRequestHeader(HttpHeaders.ContentType.Key, HttpHeaders.ContentType.Value);
			yield return request.SendWebRequest();

			HandleResponse(request, callback, out var result);
			yield return result;
		}
	}

}