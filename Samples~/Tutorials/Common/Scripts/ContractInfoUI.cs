using TezosSDK.Tezos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.Tutorials.Common
{

	public class ContractInfoUI : MonoBehaviour
	{
		[SerializeField] private TMP_InputField addressText;
		private const string NOT_CONNECTED_TEXT = "Not connected";

		private void Start()
		{
			// Subscribe to wallet events
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected += OnWalletDisconnected;
		}

		private void OnDestroy()
		{
			TezosManager.Instance.EventManager.WalletConnected -= OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected -= OnWalletDisconnected;
		}

		private void OnWalletConnected(WalletInfo walletInfo)
		{
			var contractAddress = TezosManager.Instance.Tezos.TokenContract.Address;
			addressText.text = string.IsNullOrEmpty(contractAddress) ? "Not deployed" : contractAddress;
			UpdateLayout();
		}

		private void OnWalletDisconnected(WalletInfo walletInfo)
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
			var layoutGroup = GetComponent<VerticalLayoutGroup>();

			if (layoutGroup != null)
			{
				LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.GetComponent<RectTransform>());
			}
		}
	}

}