#region

using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace TezosSDK.Contract.Scripts
{

	public class ContractInfoUI : MonoBehaviour
	{
		#region Serialized Fields

		[SerializeField] private TMP_InputField addressText;

		#endregion

		#region Constants and Fields

		private readonly string _notConnectedText = "Not connected";

		#endregion

		#region Unity Methods

		private void Start()
		{
			// Subscribe to wallet events
			TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
			TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
		}

		#endregion

		#region Event Handlers

		private void OnAccountConnected(AccountInfo accountInfo)
		{
			var contractAddress = TezosManager.Instance.Tezos.TokenContract.Address;
			addressText.text = string.IsNullOrEmpty(contractAddress) ? "Not deployed" : contractAddress;
			UpdateLayout(); // Update layout to fit the new text
		}

		private void OnAccountDisconnected(AccountInfo accountInfo)
		{
			addressText.text = _notConnectedText;
			UpdateLayout(); // Update layout to fit the new text
		}

		#endregion

		#region Private Methods

		private void UpdateLayout()
		{
			var layoutGroup = GetComponent<HorizontalLayoutGroup>();

			if (layoutGroup != null)
			{
				LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.GetComponent<RectTransform>());
			}
		}

		#endregion
	}

}