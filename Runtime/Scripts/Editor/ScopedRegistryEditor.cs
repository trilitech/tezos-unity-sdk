#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Tezos.Editor
{
	public class ScopedRegistryEditor : AssetPostprocessor
	{
		private static bool _packageImported;

		private const string _registryName   = "package.openupm.com";
		private const string _registryUrl    = "https://package.openupm.com";
		private const string _nethereumScope = "com.nethereum";
		private const string _reownScope     = "com.reown";

		private static void OnPostprocessAllAssets(
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths
			)
		{
			if (_packageImported) return;

			foreach (string asset in importedAssets)
			{
				if (asset.Contains("com.trilitech.tezos-unity-sdk"))
				{
					Debug.Log("Tezos Wallet Connect imported, adding or updating scoped registries.");
					AddOrUpdateScopedRegistry(
					                          new ScopedRegistry
					                          {
						                          name =
							                          _registryName,
						                          url    = _registryUrl,
						                          scopes = new[] { _nethereumScope, _reownScope }
					                          }
					                         );
					_packageImported = true;
					CompilationPipeline.RequestScriptCompilation();
					break;
				}
			}
		}

		private static void AddOrUpdateScopedRegistry(ScopedRegistry newRegistry)
		{
			var manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");
			var manifestJson = File.ReadAllText(manifestPath);

			var manifest = JsonConvert.DeserializeObject<ManifestJson>(manifestJson);

			var existingRegistry = manifest.scopedRegistries
			                               .FirstOrDefault(
			                                               reg => reg.name == newRegistry.name &&
			                                                      reg.url  == newRegistry.url
			                                              );

			if (existingRegistry != null)
			{
				var missingScopes = newRegistry.scopes.Except(existingRegistry.scopes).ToList();
				if (missingScopes.Count > 0)
				{
					existingRegistry.scopes = existingRegistry.scopes.Concat(missingScopes).ToArray();
					Debug.Log(
					          $"Updated scoped registry '{newRegistry.name}' with missing scopes: {string.Join(", ", missingScopes)}"
					         );
				}
				else
				{
					Debug.Log(
					          $"No updates needed for scoped registry '{newRegistry.name}' as all scopes are already present."
					         );
				}
			}
			else
			{
				manifest.scopedRegistries.Add(newRegistry);
				Debug.Log(
				          $"Added new scoped registry '{newRegistry.name}' with scopes: {string.Join(", ", newRegistry.scopes)}"
				         );
			}

			File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
		}

		public class ScopedRegistry
		{
			public string   name;
			public string   url;
			public string[] scopes;
		}

		public class ManifestJson
		{
			public Dictionary<string, string> dependencies     = new();
			public List<ScopedRegistry>       scopedRegistries = new();
		}
	}
}
#endif