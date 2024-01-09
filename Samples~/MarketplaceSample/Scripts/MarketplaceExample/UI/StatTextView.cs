using TMPro;
using UnityEngine;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class StatTextView : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI statNameText;
		[SerializeField] private TextMeshProUGUI statValueText;

		public void SetStatName(string name)
		{
			statNameText.text = name;
		}

		public void SetStatValue(string value)
		{
			statValueText.text = value;
		}
	}

}