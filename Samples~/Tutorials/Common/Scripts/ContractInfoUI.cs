using Tezos.API;
using Tezos.WalletProvider;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.Samples.Tutorials.Common
{

	public class ContractInfoUI : MonoBehaviour
	{
		[SerializeField] private TMP_InputField addressText;
		private const string NOT_CONNECTED_TEXT = "Not connected";

		private void Start()
		{
			// Subscribe to wallet events
			TezosAPI.WalletConnected += OnWalletConnected;
			TezosAPI.WalletDisconnected += OnWalletDisconnected;
		}

		private void OnDestroy()
		{
			TezosAPI.WalletConnected -= OnWalletConnected;
			TezosAPI.WalletDisconnected -= OnWalletDisconnected;
		}

		private void OnWalletConnected(WalletProviderData walletProviderData)
		{
			// var contractAddress = TezosAPI.TokenContract.Address;
			// addressText.text = string.IsNullOrEmpty(contractAddress) ? "Not deployed" : contractAddress;
			// UpdateLayout();
		}

		private void OnWalletDisconnected()
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