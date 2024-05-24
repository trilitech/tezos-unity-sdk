using TezosSDK.Helpers.HttpClients;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
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
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected += OnWalletDisconnected;
		}

		private void OnWalletConnected(WalletInfo _)
		{
			var routine = TezosManager.Instance.Tezos.GetCurrentWalletBalance(OnBalanceResult);
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

		private void OnBalanceResult(HttpResult<ulong> result)
		{
			if (result.Success)
			{
				OnBalanceFetched(result.Data);
			}
			else
			{
				TezosLogger.LogError(result.ErrorMessage);
			}
		}
	}

}