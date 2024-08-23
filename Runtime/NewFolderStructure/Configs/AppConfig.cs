using UnityEngine;

namespace TezosSDK.Configs
{
	[CreateAssetMenu(fileName = "AppConfig", menuName = "Tezos/Configuration/AppConfig", order = 1)]
	public class AppConfig: ScriptableObject
	{
		public string appName        = "Default App Name";
		public string appUrl         = "https://tezos.com";
		public string appIcon        = "https://tezos.com/favicon.ico";
		public string appDescription = "App Description";
	}
}
