using Tezos.Configs;
using Tezos.MessageSystem;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.WalletConnection
{

	public class MetadataInfoUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI nameText;
		[SerializeField] private TextMeshProUGUI descriptionText;

		private void Start()
		{
			var appConfig   = ConfigGetter.GetOrCreateConfig<AppConfig>();
			
			nameText.text = appConfig.AppName;
			descriptionText.text = appConfig.AppDescription;
		}
	}

}