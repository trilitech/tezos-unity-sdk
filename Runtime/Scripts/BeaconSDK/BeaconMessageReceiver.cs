using System;
using System.Collections;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BeaconSDK
{
	/// <summary>
	/// Receives external messages
	/// </summary>

	public class BeaconMessageReceiver : MonoBehaviour
	{
		public event Action<string> ClientCreated;
		public event Action<string> AccountConnected;
		public event Action<string> AccountConnectionFailed;
		public event Action<string> AccountDisconnected;
		public event Action<string> ContractCallCompleted;
		public event Action<string> ContractCallInjected;
		public event Action<string> ContractCallFailed;
		public event Action<string> PayloadSigned;
		public event Action<string> HandshakeReceived;
		public event Action<string> PairingCompleted;
		public event Action<string> AccountReceived;

		public void OnClientCreated(string result)
		{
			Debug.LogWarning("From unity, OnClientCreated: " + result);
			ClientCreated?.Invoke(result);
		}

		public void OnAccountConnected(string address)
		{
			// result is the json permission response
			Debug.Log("From unity, OnAccountConnected: " + address);
			AccountConnected?.Invoke(address);
		}

		public void OnAccountFailedToConnect(string result)
		{
			// result is the json error
			Debug.Log("From unity, OnAccountFailedToConnect: " + result);
			AccountConnectionFailed?.Invoke(result);
		}
	
		public void OnAccountDisconnected(string result)
		{
			Debug.Log("From unity, OnAccountDisconnect: " + result);
			AccountDisconnected?.Invoke(result);
		}

		public void OnContractCallCompleted(string result)
		{
			// result is the json of transaction response
			Debug.Log("From unity, OnContractCallCompleted: " + result);
			ContractCallCompleted?.Invoke(result);
		}

		[Serializable]
		struct ContractCallInjectionResult
		{
			public bool success;
			public string transactionHash;
		}

		public IEnumerator ContractCallInjection(string uri, string transactionHash)
		{
			var success = false;
			var timeout = 30f; // seconds
			var timestamp = Time.time;

			// keep making requests until time out or success
			while (!success && Time.time - timestamp < timeout)
			{
				uri = string.Format(uri, transactionHash);
				Debug.Log("WebRequest into " + uri);
				using UnityWebRequest webRequest = UnityWebRequest.Get(uri);
				yield return webRequest.SendWebRequest();

				var resultText = webRequest.downloadHandler.text;

				if (!string.IsNullOrEmpty(webRequest.error))
				{
					Debug.LogError(webRequest.error);
					break;
				}

				if (!string.IsNullOrEmpty(resultText))
				{
					success = JsonSerializer.Deserialize<bool>(resultText);
				}
			}

			ContractCallInjectionResult result;
			result.success = success;
			result.transactionHash = transactionHash;
			Debug.Log("Operation injected into blockchain: " + success);
			ContractCallInjected?.Invoke(JsonUtility.ToJson(result));
		}

		public void OnContractCallFailed(string result)
		{
			// result is error or empty
			Debug.Log("From unity, OnContractCallFailed: " + result);
			ContractCallFailed?.Invoke(result);
		}
	
		public void OnPayloadSigned(string signature)
		{
			// result is the json string of payload signing result
			Debug.Log("From unity, OnPayloadSigned: " + signature);
			PayloadSigned?.Invoke(signature);
		}

		public void OnHandshakeReceived(string handshake)
		{
			// result is serialized p2p pairing request
			Debug.Log("From unity, OnHandshakeReceived: " + handshake);
			HandshakeReceived?.Invoke(handshake);
		}
	
		public void OnPairingCompleted(string message)
		{
			Debug.Log("From unity, OnPairingCompleted: " + message);
			PairingCompleted?.Invoke(message);
		}

		public void OnAccountReceived(string message)
		{
			Debug.Log("From unity, OnAccountReceived: " + message);
			AccountReceived?.Invoke(message);
		}
	}
}
