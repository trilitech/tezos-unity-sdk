using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

namespace TezosSDK.Tutorials.WalletConnection
{

	public class WalletBalanceUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI balanceText;
		private readonly string _notConnectedText = "Not connected";

		private void Start()
		{
			// Subscribe to wallet events
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected += OnWalletDisconnected;
		}

		private void OnWalletConnected(WalletInfo _)
		{
			// OnBalanceFetched will be called when the balance is fetched
			var routine = TezosManager.Instance.Tezos.GetCurrentWalletBalance(OnBalanceFetched);
			StartCoroutine(routine);
		}

		private void OnWalletDisconnected(WalletInfo _)
		{
			balanceText.text = _notConnectedText;
		}

		private void OnBalanceFetched(ulong balance)
		{
			// Balance is in microtez, so we divide it by 1.000.000 to get tez
			balanceText.text = $"{balance / 1000000f}";
		}
	}

}