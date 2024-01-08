using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.NftApiExample
{

	public class UiManager : MonoBehaviour
	{
		[SerializeField] private DataManager dataManager;
		[SerializeField] private InputField addressInputField;
		[SerializeField] private InputField contractInputField;
		[SerializeField] private InputField tokenIdInputField;
		[SerializeField] private Text resultText;

		private void Start()
		{
			dataManager.DataReceived += OnDataReceived;

			addressInputField.onEndEdit.AddListener(delegate { OnEndEditAddress(addressInputField); });
			contractInputField.onEndEdit.AddListener(delegate { OnEndEditContract(contractInputField); });
			tokenIdInputField.onEndEdit.AddListener(delegate { OnEndEditTokenId(tokenIdInputField); });
		}

		private void OnDataReceived(string data)
		{
			resultText.text = string.Empty;
			resultText.text = data;
		}

		private void OnEndEditAddress(InputField input)
		{
			dataManager.SetCheckAddress(input.text);
		}

		private void OnEndEditContract(InputField input)
		{
			dataManager.SetCheckContract(input.text);
		}

		private void OnEndEditTokenId(InputField input)
		{
			dataManager.SetCheckTokenId(input.text);
		}
	}

}