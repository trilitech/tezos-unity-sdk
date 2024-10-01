using System.Threading.Tasks;
using Newtonsoft.Json;
using Tezos.Common;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MainThreadDispatcher;
using Tezos.MessageSystem;
using UnityEngine;

namespace Tezos.SaveSystem
{
	public class SaveController : IController
	{
		public bool IsInitialized { get; private set; }

		public UniTask Initialize(IContext context)
		{
			IsInitialized = true;
			return UniTask.CompletedTask;
		}

		public async UniTask Save<T>(string key, T data)
		{
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(
			                                                        () =>
			                                                        {
				                                                        TezosLogger.LogInfo($"Saving data to {key}");
				                                                        var serializedObject = JsonConvert.SerializeObject(data);
				                                                        PlayerPrefs.SetString(key, serializedObject);
				                                                        TezosLogger.LogInfo($"Data saved");
			                                                        }
			                                                       );
		}

		public async Task<T> Load<T>(string key)
		{
			T loadedData = default;
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(
			                                                        () =>
			                                                        {
				                                                        TezosLogger.LogInfo($"Loading data from {key}");
				                                                        loadedData = JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(key, default));
				                                                        TezosLogger.LogInfo($"Loaded data from {key}");
			                                                        }
			                                                       );

			return loadedData;
		}

		public void Delete(string key)
		{
			TezosLogger.LogInfo($"Deleting data from {key}");
			PlayerPrefs.DeleteKey(key);
			TezosLogger.LogInfo($"Deleted data from {key}");
		}
	}
}