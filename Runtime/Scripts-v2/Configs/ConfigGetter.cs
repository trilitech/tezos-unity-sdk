using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tezos.MessageSystem
{
	public static class ConfigGetter
	{
		private const           string                               CONFIG_PATH = "Assets/Tezos/Resources";
		private static readonly Dictionary<string, ScriptableObject> _cache      = new();

		public static T GetOrCreateConfig<T>() where T : ScriptableObject
		{
			string key = typeof(T).Name;

			// Check if the config is already in cache
			if (_cache.TryGetValue(key, out ScriptableObject cachedConfig))
			{
				if(cachedConfig != null)
					return cachedConfig as T;
			}

			// Try to load the ScriptableObject from resources
			T config = Resources.Load<T>(key);

			// If not found, create it
			if (config == null)
			{
				config = ScriptableObject.CreateInstance<T>();

#if UNITY_EDITOR
				// Ensure the directory exists
				string assetPath     = Path.Combine(CONFIG_PATH, $"{typeof(T).Name}.asset");
				string directoryPath = Path.GetDirectoryName(assetPath);

				if (!Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}

				// Save the ScriptableObject as an asset in the Resources folder
				AssetDatabase.CreateAsset(config, assetPath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
#else
                Debug.LogError("Config could not be found, and new config creation is only supported in the Unity Editor.");
#endif
			}

			// Cache the config
			_cache[key] = config;

			return config;
		}
	}
}