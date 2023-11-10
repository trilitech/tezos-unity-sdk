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

		private void OnAccountDisconnected(string obj)
		{
			addressText.text = notConnectedText;
		}

		private void OnAccountConnected(string obj)
		{
			addressText.text = TezosManager.Instance.Wallet.GetActiveAddress();
		}
	}

}