#region

using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace TezosSDK.Common.Scripts
{

	public class ContractInfoUI : MonoBehaviour
	{
		[SerializeField] private TMP_InputField addressText;
		private const string NOT_CONNECTED_TEXT = "Not connected";

		private void Start()
		{
			// Subscribe to wallet events
			TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
			TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
		}

		private void OnAccountConnected(AccountInfo accountInfo)
		{
			var contractAddress = TezosManager.Instance.Tezos.TokenContract.Address;
			addressText.text = string.IsNullOrEmpty(contractAddress) ? "Not deployed" : contractAddress;
			UpdateLayout();
		}

		private void OnAccountDisconnected(AccountInfo accountInfo)
		{
			addressText.text = NOT_CONNECTED_TEXT;
			UpdateLayout();
		}

		public void SetAddress(string address)
		{
			addressText.text = address;
			UpdateLayout();
		}

		private void UpdateLayout() // Update layout to fit the new text
		{
			var layoutGroup = GetComponent<HorizontalLayoutGroup>();

			if (layoutGroup != null)
			{
				LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.GetComponent<RectTransform>());
			}
		}
	}

}