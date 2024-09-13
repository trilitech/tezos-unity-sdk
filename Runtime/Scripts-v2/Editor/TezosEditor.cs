#if UNITY_EDITOR
using TezosSDK.Configs;
using TezosSDK.MessageSystem;
using UnityEditor;
using UnityEngine;

namespace NewFolderStructure.Editor
{
    public class TezosEditor : EditorWindow
    {
        [MenuItem("Tezos/Setup Configs")]
        private static void SetupTezosConfigs()
        {
            ConfigGetter.GetOrCreateConfig<AppConfig>();
            ConfigGetter.GetOrCreateConfig<DataProviderConfig>();
            TezosConfig tezosConfig = ConfigGetter.GetOrCreateConfig<TezosConfig>();
            Debug.Log($"Tezos configs setup");
            EditorGUIUtility.PingObject(tezosConfig);
        }
    }
}
#endif