#region

using TezosSDK.Beacon;
using TezosSDK.Tezos;
using UnityEngine;

#endregion

namespace TezosSDK.Examples.WalletConnection.Scripts
{

	public class ConnectedTextUI : MonoBehaviour
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

		private void OnAccountConnected(AccountInfo accountInfo)
		{
			gameObject.SetActive(true);
		}

		private void OnAccountDisconnected(AccountInfo accountInfo)
		{
			gameObject.SetActive(false);
		}
	}

}