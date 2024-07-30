using System.Collections.Generic;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.WalletServices.Connectors;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TezosSDK.Samples.Tutorials.WalletConnection
{

	public class WalletConnectionHandler : MonoBehaviour
	{
		[SerializeField] private TMP_InputField nameText;
		[SerializeField] private TMP_InputField descriptionText;
		[SerializeField] private TMP_InputField balanceText;
		[SerializeField] private TMP_InputField kukaiTypeOfLoginText;
		[SerializeField] private TextMeshProUGUI kukaiSignedMessageText;
		[SerializeField] private List<GameObject> kukaiOnlyObjects;
		

		private void Start()
		{
			nameText.text = TezosManager.Instance.DAppMetadata.Name;
			descriptionText.text = TezosManager.Instance.DAppMetadata.Description;
			
			CheckKukaiOnlyObjects();

			// Subscribe to wallet events
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected += OnWalletDisconnected;
		}

		// Check if the wallet is Kukai and disable objects on the scene that are only for Kukai if it's not
		private void CheckKukaiOnlyObjects()
		{
			if (TezosManager.Instance.WalletConnector.ConnectorType == ConnectorType.Kukai)
			{
				return;
			}

			foreach (var obj in kukaiOnlyObjects)
			{
				obj.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			TezosManager.Instance.EventManager.WalletConnected -= OnWalletConnected;
			TezosManager.Instance.EventManager.WalletDisconnected -= OnWalletDisconnected;
		}

		private void OnWalletConnected(WalletInfo _)
		{
			HandleKukaiOnlyObjects();

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
					TezosLogger.LogError(result.ErrorMessage);
				}
			}));
		}

		// If the wallet is Kukai, display additional information
		private void HandleKukaiOnlyObjects()
		{
			if (TezosManager.Instance.WalletConnector.ConnectorType != ConnectorType.Kukai)
			{
				return;
			}

			KukaiConnector kukaiConnector = (KukaiConnector)TezosManager.Instance.WalletConnector;
			kukaiTypeOfLoginText.text = kukaiConnector.TypeOfLogin.ToString();
			kukaiSignedMessageText.text = kukaiConnector.AuthResponse.Message;
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