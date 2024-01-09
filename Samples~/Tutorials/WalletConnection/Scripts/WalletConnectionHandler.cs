using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

namespace TezosSDK.Tutorials.WalletConnection
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
			// OnBalanceFetched will be called when the balance is fetched
			var routine = TezosManager.Instance.Tezos.GetCurrentWalletBalance(OnBalanceFetched);
			StartCoroutine(routine);
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