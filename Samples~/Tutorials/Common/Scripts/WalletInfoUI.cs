using Tezos.API;
using Tezos.WalletProvider;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.Common
{

	public class WalletInfoUI : MonoBehaviour
	{
		[SerializeField] private TMP_InputField addressText;
		private const string NOT_CONNECTED_TEXT = "Not connected";

		private void Start()
		{
			addressText.text = NOT_CONNECTED_TEXT;

			TezosAPI.WalletConnected += OnWalletConnected;
			TezosAPI.WalletDisconnected += OnWalletDisconnected;
		}

		private void OnDestroy()
		{
			TezosAPI.WalletConnected -= OnWalletConnected;
			TezosAPI.WalletDisconnected -= OnWalletDisconnected;
		}

		private void OnWalletConnected(WalletProviderData walletProviderData)
		{
			// We can get the address from the event data
			addressText.text = walletProviderData.WalletAddress;
		}

		private void OnWalletDisconnected()
		{
			addressText.text = NOT_CONNECTED_TEXT;
		}
	}

}