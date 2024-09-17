using UnityEngine;
using UnityEngine.Serialization;

namespace Tezos.Configs
{
	[CreateAssetMenu(fileName = "AppConfig", menuName = "Tezos/Configuration/AppConfig", order = 1)]
	public class AppConfig: ScriptableObject
	{
		public string AppName        = "Default App Name";
		public string AppUrl         = "https://tezos.com";
		public string AppIcon        = "https://tezos.com/favicon.ico";
		public string AppDescription = "App Description";
	}
}
