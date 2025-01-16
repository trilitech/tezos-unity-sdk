using Tezos.API;
using Tezos.WalletProvider;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.WalletConnection
{

	public class WalletBalanceUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI balanceText;
		private readonly string _notConnectedText = "Not connected";

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

		private async void OnWalletConnected(WalletProviderData walletProviderData)
		{
			// Balance is in microtez, so we divide it by 1.000.000 to get tez
			var balance          = ulong.Parse(await TezosAPI.GetBalance());
			int convertedBalance = (int)(balance / 1000000);
			balanceText.text = convertedBalance + " XTZ";
		}

		private void OnWalletDisconnected()
		{
			balanceText.text = _notConnectedText;
		}
	}

}