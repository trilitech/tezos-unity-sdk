#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Tezos.Configs;
using Tezos.MessageSystem;
using Tezos.Reflection;
using UnityEditor;
using UnityEngine;

namespace Tezos.Editor
{
	public class TezosEditor : EditorWindow, ITezosEditor
	{
		private const string LinkerFolderPath          = "Assets/Tezos/Linker";
		private const string LinkerFileName            = "link.xml";
		private const string SourceStreamingAssetsPath = "Packages/com.trilitech.tezos-unity-sdk/WebGLFrontend/output/StreamingAssets";
		private const string SourceWebGLTemplatesPath  = "Packages/com.trilitech.tezos-unity-sdk/WebGLFrontend/output/WebGLTemplates";
		private const string DestinationAssetsPath     = "Assets";
		private const string WebGLTemplateName         = "Airgap";

		[MenuItem("Tezos/Setup Configs")]
		public static void SetupTezosConfigs()
		{
			var tezosEditors = ReflectionHelper.CreateInstancesOfType<ITezosEditor>().ToList();
			tezosEditors.ForEach(editor => editor.SetupConfigs());
		}

		public void SetupConfigs()
		{
			CreateLinkerFile();
			CopyAndMergeFolders(SourceStreamingAssetsPath, Path.Combine(DestinationAssetsPath, "StreamingAssets"));
			CopyAndMergeFolders(SourceWebGLTemplatesPath,  Path.Combine(DestinationAssetsPath, "WebGLTemplates"));
			SetWebGLTemplate(WebGLTemplateName);
			CreateTezosConfigs();
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Debug.Log("Tezos setup completed.");
		}

		private static void CreateLinkerFile()
		{
			if (!Directory.Exists(LinkerFolderPath))
			{
				Directory.CreateDirectory(LinkerFolderPath);
				Debug.Log($"Created directory: {LinkerFolderPath}");
			}

			string linkerFilePath = Path.Combine(LinkerFolderPath, LinkerFileName);

			string linkXmlContent = @"<linker>
<!--Preserve an entire assembly-->
    <assembly fullname=""Tezos.Initializer"" preserve=""all""/>
    <assembly fullname=""Tezos.WalletProvider"" preserve=""all""/>
    <assembly fullname=""Tezos.SocialLoginProvider"" preserve=""all""/>
</linker>";

			File.WriteAllText(linkerFilePath, linkXmlContent);
			Debug.Log($"link.xml created/updated at: {linkerFilePath}");
		}

		private static void CopyAndMergeFolders(string sourcePath, string destinationPath)
		{
			if (!Directory.Exists(sourcePath))
			{
				Debug.LogError($"Source folder does not exist: {sourcePath}");
				return;
			}

			if (!Directory.Exists(destinationPath))
			{
				Directory.CreateDirectory(destinationPath);
				Debug.Log($"Created destination folder: {destinationPath}");
			}

			CopyDirectoryRecursively(sourcePath, destinationPath);
		}

		private static void CopyDirectoryRecursively(string sourceDir, string targetDir)
		{
			foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
			{
				string targetDirectory = dirPath.Replace(sourceDir, targetDir);
				if (!Directory.Exists(targetDirectory))
				{
					Directory.CreateDirectory(targetDirectory);
					Debug.Log($"Created directory: {targetDirectory}");
				}
			}

			foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
			{
				string targetFilePath = filePath.Replace(sourceDir, targetDir);
				File.Copy(filePath, targetFilePath, true);
				Debug.Log($"Copied file: {targetFilePath}");
			}
		}

		private static void SetWebGLTemplate(string templateName)
		{
			PlayerSettings.WebGL.template = templateName;
			Debug.Log($"Set WebGL template to: {templateName}");
		}

		private static void CreateTezosConfigs()
		{
			ConfigGetter.GetOrCreateConfig<AppConfig>();
			DataProviderConfig dataProviderConfig = ConfigGetter.GetOrCreateConfig<DataProviderConfig>();
			TezosConfig        tezosConfig        = ConfigGetter.GetOrCreateConfig<TezosConfig>();

			tezosConfig.DataProvider = dataProviderConfig;

			EditorUtility.SetDirty(tezosConfig);
			Debug.Log("Tezos configs created and updated.");

			EditorGUIUtility.PingObject(tezosConfig);
		}
	}

	public class TezosPackageImportListener : AssetPostprocessor
	{
		private static bool _packageImported;

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (_packageImported) return;

			foreach (string asset in importedAssets)
			{
				if (asset.Contains("com.trilitech.tezos-unity-sdk"))
				{
					Debug.Log("Tezos Unity SDK imported, running setup...");
					TezosEditor.SetupTezosConfigs();
					_packageImported = true;
					break;
				}
			}
		}
	}
}
#endif