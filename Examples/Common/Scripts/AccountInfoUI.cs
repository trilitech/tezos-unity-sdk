using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.Examples.WalletConnection.Scripts
{

	public class AccountInfoUI : MonoBehaviour
	{
		[SerializeField] private TMP_InputField addressText;

		private readonly string _notConnectedText = "Not connected";

		private void Start()
		{
			addressText.text = _notConnectedText;

			// Subscribe to events
			TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
			TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
		}

		private void OnAccountConnected(AccountInfo accountInfo)
		{
			// We can get the address from the wallet
			addressText.text = TezosManager.Instance.Wallet.GetActiveAddress();
			// Or from the event data
			addressText.text = accountInfo.Address;

			UpdateLayout(); // Update layout to fit the new text
		}

		private void OnAccountDisconnected(AccountInfo accountInfo)
		{
			addressText.text = _notConnectedText;
			UpdateLayout();
		}

		private void UpdateLayout()
		{
			var layoutGroup = GetComponent<HorizontalLayoutGroup>();

			if (layoutGroup != null)
			{
				LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.GetComponent<RectTransform>());
			}
		}
	}

}