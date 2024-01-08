using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

namespace TezosSDK.Tutorials.WalletConnection
{

	public class MetadataInfoUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI nameText;
		[SerializeField] private TextMeshProUGUI descriptionText;

		private void Start()
		{
			nameText.text = TezosManager.Instance.DAppMetadata.Name;
			descriptionText.text = TezosManager.Instance.DAppMetadata.Description;
		}
	}

}