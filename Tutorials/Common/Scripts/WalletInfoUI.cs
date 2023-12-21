#region

using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

#endregion

namespace TezosSDK.Examples.WalletConnection.Scripts
{

	public class WalletInfoUI : MonoBehaviour
	{
		[SerializeField] private TMP_InputField addressText;
		private const string NOT_CONNECTED_TEXT = "Not connected";

		private void Start()
		{
			addressText.text = NOT_CONNECTED_TEXT;

			// Subscribe to events
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected += OnWalletDisconnected;
		}

		private void OnWalletConnected(WalletInfo walletInfo)
		{
			// We can get the address from the wallet
			addressText.text = TezosManager.Instance.Wallet.GetActiveAddress();
			// Or from the event data
			addressText.text = walletInfo.Address;
		}

		private void OnWalletDisconnected(WalletInfo walletInfo)
		{
			addressText.text = NOT_CONNECTED_TEXT;
		}
	}

}