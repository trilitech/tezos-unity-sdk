using TezosSDK.Beacon;
using TezosSDK.Tezos;
using UnityEngine;

namespace TezosSDK.Examples.WalletConnection.Scripts
{

	public class LogoutButton : MonoBehaviour
	{
		private void Start()
		{
			TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
			TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;

			// Hide the button if there is no active account
			if (string.IsNullOrEmpty(TezosManager.Instance.Wallet.GetActiveAddress()))
			{
				gameObject.SetActive(false);
			}
		}

		private void OnAccountConnected(AccountInfo account_info)
		{
			gameObject.SetActive(true);
		}

		private void OnAccountDisconnected(AccountInfo account_info)
		{
			gameObject.SetActive(false);
		}

		public void Logout()
		{
			TezosManager.Instance.Wallet.Disconnect();
		}
	}

}