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

		private void OnAccountDisconnected(string obj)
		{
			gameObject.SetActive(false);
		}

		private void OnAccountConnected(string obj)
		{
			gameObject.SetActive(true);
		}

		public void Logout()
		{
			TezosManager.Instance.Wallet.Disconnect();
		}
		
		
		
	}

}