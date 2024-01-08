using System.Collections.Generic;
using System.Linq;
using TezosSDK.Tezos.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class InitiateContractController : MonoBehaviour
	{
		[SerializeField] private ToggleGroup contractsToggleGroup;
		[SerializeField] private GameObject itemPrefab;
		[SerializeField] private UIManager uiManager;

		public void DrawContractToggles(IEnumerable<TokenContract> contracts, string walletAddress)
		{
			foreach (Transform child in contractsToggleGroup.transform)
			{
				Destroy(child.gameObject);
			}

			var activeContractAddress = PlayerPrefs.GetString("CurrentContract:" + walletAddress);

			foreach (var c in contracts)
			{
				var newItem = Instantiate(itemPrefab, contractsToggleGroup.transform);
				var toggle = newItem.GetComponent<Toggle>();
				toggle.group = contractsToggleGroup;
				toggle.isOn = activeContractAddress == c.Address;
				newItem.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = c.Address;
			}
		}

		public Toggle GetSelectedToggle()
		{
			var toggles = contractsToggleGroup.GetComponentsInChildren<Toggle>();
			return toggles.FirstOrDefault(t => t.isOn);
		}

		public void InitiateContract()
		{
			var contractAddress = GetSelectedToggle().gameObject.GetComponentInChildren<TextMeshProUGUI>().text;

			uiManager.InitContract(contractAddress);
		}
	}

}