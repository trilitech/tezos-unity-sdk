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
		protected TezosHttpClient(int timeOut)
		{
			RequestTimeout = timeOut;
		}

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

		public async Task<T> GetRequest<T>(string endpoint)
		{
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

		public async Task<T> PostRequest<T>(string endpoint, object data)
		{
			TezosLogger.LogDebug($"POST: {endpoint}");
			
			using HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add(HttpHeaders.Accept.Key, HttpHeaders.Accept.Value);
			client.DefaultRequestHeaders.Add(HttpHeaders.UserAgent.Key, HttpHeaders.UserAgent.Value);
			client.DefaultRequestHeaders.Add(HttpHeaders.ContentType.Key, HttpHeaders.ContentType.Value);

			HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));

			// Ensure we received a successful response
			response.EnsureSuccessStatusCode();

			// Parse the response as JSON and extract the balance
			string responseBody = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<T>(responseBody);
		}
	}

}