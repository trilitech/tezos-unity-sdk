using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

namespace TezosSDK.Examples.WalletConnection.Scripts
{

	public class AccountInfoUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI addressText;
		
		private readonly string notConnectedText = "Not connected";

		private void Start()
		{
			addressText.text = notConnectedText;
			TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
			TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
		}

		private void OnAccountDisconnected(AccountInfo account_info)
		{
			addressText.text = notConnectedText;
		}

		private void OnAccountConnected(AccountInfo account_info)
		{
			addressText.text = TezosManager.Instance.Wallet.GetActiveAddress();
			// OR
			addressText.text = account_info.Address;
		}
	}
}