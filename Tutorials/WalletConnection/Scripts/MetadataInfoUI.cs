#region

using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

#endregion

namespace TezosSDK.Tutorials.WalletConnection.Scripts
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