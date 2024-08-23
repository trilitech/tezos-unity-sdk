using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TezosSDK.MessageSystem
{
	public static class ConfigGetter
	{
		private const           string                               CONFIG_PATH = "Tezos";
		private static readonly Dictionary<string, ScriptableObject> _cache      = new();

		public static T GetOrCreateConfig<T>() where T: ScriptableObject
		{
			string key = nameof(T);

			// Check if the config is already in cache
			if (_cache.TryGetValue(key, out ScriptableObject cachedConfig))
			{
				return cachedConfig as T;
			}

			// Try to load the ScriptableObject from resources
			T config = Resources.Load<T>(CONFIG_PATH);

			// If not found, create it
			if (config == null)
			{
				config = ScriptableObject.CreateInstance<T>();

#if UNITY_EDITOR
				// Ensure the directory exists
				string assetPath     = $"Assets/Resources/{CONFIG_PATH}/{nameof(T)}.asset";
				string directoryPath = System.IO.Path.GetDirectoryName(assetPath);

				if (!System.IO.Directory.Exists(directoryPath))
				{
					System.IO.Directory.CreateDirectory(directoryPath);
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
