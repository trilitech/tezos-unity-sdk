using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.WalletConnection
{

	public class WalletConnectionHandler : MonoBehaviour
	{
		[SerializeField] private TMP_InputField nameText;
		[SerializeField] private TMP_InputField descriptionText;
		[SerializeField] private TMP_InputField balanceText;

		private void Start()
		{
			nameText.text = TezosManager.Instance.DAppMetadata.Name;
			descriptionText.text = TezosManager.Instance.DAppMetadata.Description;

			// Subscribe to wallet events
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected += OnWalletDisconnected;
		}

		private void OnDestroy()
		{
			TezosManager.Instance.EventManager.WalletConnected -= OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected -= OnWalletDisconnected;
		}

		private void OnWalletConnected(WalletInfo _)
		{
			StartCoroutine(TezosManager.Instance.Tezos.GetCurrentWalletBalance(result =>
			{
				if (result.Success)
				{
					// Handle the successful retrieval of the wallet balance
					OnBalanceFetched(result.Data);
				}
				else
				{
					// Handle the error case, update UI or log error
					TezosLog.Error(result.ErrorMessage);
				}
			}));
		}

		private void OnWalletDisconnected(WalletInfo _)
		{
			balanceText.text = "";
		}

		private void OnBalanceFetched(ulong balance)
		{
			// Balance is in microtez, so we divide it by 1.000.000 to get tez
			balanceText.text = $"{balance / 1000000f}";
		}
	}

}