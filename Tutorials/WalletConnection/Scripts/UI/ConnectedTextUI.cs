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
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected += OnWalletDisconnected;

			// Hide the button if there is no active account
			if (string.IsNullOrEmpty(TezosManager.Instance.Wallet.GetActiveAddress()))
			{
				gameObject.SetActive(false);
			}
		}

		private void OnWalletConnected(WalletInfo walletInfo)
		{
			gameObject.SetActive(true);
		}

		private void OnWalletDisconnected(WalletInfo walletInfo)
		{
			gameObject.SetActive(false);
		}
	}

}