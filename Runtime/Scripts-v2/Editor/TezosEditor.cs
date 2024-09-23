#if UNITY_EDITOR
using System.IO;
using Tezos.Configs;
using Tezos.MessageSystem;
using UnityEditor;
using UnityEngine;

namespace NewFolderStructure.Editor
{
	public class TezosEditor : EditorWindow
	{
		private const string LinkerFolderPath = "Assets/Tezos/Linker";
		private const string LinkerFileName   = "link.xml";

		[MenuItem("Tezos/Setup Configs")]
		private static void SetupTezosConfigs()
		{
			CreateLinkerFile();

			ConfigGetter.GetOrCreateConfig<AppConfig>();
			DataProviderConfig dataProviderConfig = ConfigGetter.GetOrCreateConfig<DataProviderConfig>();
			TezosConfig        tezosConfig        = ConfigGetter.GetOrCreateConfig<TezosConfig>();

			tezosConfig.DataProvider = dataProviderConfig;
			EditorUtility.SetDirty(tezosConfig);
			AssetDatabase.SaveAssetIfDirty(tezosConfig);
			AssetDatabase.Refresh();
			Debug.Log($"Tezos configs setup");
			EditorGUIUtility.PingObject(tezosConfig);
		}

		private static void CreateLinkerFile()
		{
			if (!Directory.Exists(LinkerFolderPath))
			{
				Directory.CreateDirectory(LinkerFolderPath);
				Debug.Log($"Created directory: {LinkerFolderPath}");
			}

			string linkerFilePath = Path.Combine(LinkerFolderPath, LinkerFileName);

			// Define the content of the link.xml
			string linkXmlContent = @"<linker>
    <!--Preserve an entire assembly-->
    <assembly fullname=""Tezos.Initializer"" preserve=""all""/>
</linker>";

			File.WriteAllText(linkerFilePath, linkXmlContent);
			Debug.Log($"link.xml created/updated at: {linkerFilePath}");

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}
#endif