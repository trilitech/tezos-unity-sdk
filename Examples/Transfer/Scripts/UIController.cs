using System.Linq;
using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models.Filters;
using TMPro;
using UnityEngine;

namespace TezosSDK.Transfer.Scripts
{

	public class UIController : MonoBehaviour
	{
		[SerializeField] private GameObject transferControls;
		[SerializeField] private TextMeshProUGUI tokenIdsText;

		private void Start()
		{
			// Subscribe to account connection events
			TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnexted;
			TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
			
			transferControls.SetActive(false);
		}
		
		private void OnAccountDisconnected(AccountInfo _)
		{
			transferControls.SetActive(false);
		}

		private void OnAccountConnexted(AccountInfo _)
		{
			transferControls.SetActive(true);

			var contractAddress = TezosManager.Instance.Tezos.TokenContract.Address;

			if (string.IsNullOrEmpty(contractAddress))
			{
				return;
			}

			var tokensForContractCoroutine = TezosManager.Instance.Tezos.API.GetTokensForContract(tokens =>
			{
				var idsResult = tokens.Aggregate(string.Empty, (resultString, token) => $"{resultString}{token.TokenId}, ");

				tokenIdsText.text = idsResult[..^2];
			}, contractAddress, false, 10_000, new TokensForContractOrder.Default(0));

			StartCoroutine(tokensForContractCoroutine);
		}
	}

}