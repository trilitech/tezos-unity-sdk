#region

using System.Collections.Generic;
using System.Linq;
using TezosSDK.Beacon;
using TezosSDK.Common.Scripts;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using TMPro;
using UnityEngine;

#endregion

namespace TezosSDK.Transfer.Scripts
{

	public class UIController : MonoBehaviour
	{
		[SerializeField] private GameObject transferControls;
		[SerializeField] private TextMeshProUGUI tokenIdsText;
		[SerializeField] private ContractInfoUI contractInfoUI;

		private void Start()
		{
			// Subscribe to account connection events
			TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
			TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;

			transferControls.SetActive(false);
		}

		private void OnAccountConnected(AccountInfo _)
		{
			transferControls.SetActive(true);

			var contractAddress = TezosManager.Instance.Tezos.TokenContract.Address;

			if (!string.IsNullOrEmpty(contractAddress))
			{
				GetContractTokenIds(contractAddress);
				return;
			}

			var getOriginatedContractsRoutine = TezosManager.Instance.Tezos.GetOriginatedContracts(contracts =>
			{
				var tokenContracts = contracts.ToList();

				if (!tokenContracts.Any())
				{
					var activeAddress = TezosManager.Instance.Tezos.Wallet.GetActiveAddress();

					tokenIdsText.text = $"{activeAddress} didn't deploy any contract yet.";
					return;
				}

				var initializedContract = tokenContracts.First();
				TezosManager.Instance.Tezos.TokenContract = initializedContract;

				contractInfoUI.SetAddress(initializedContract.Address);
				GetContractTokenIds(initializedContract.Address);
			});

			StartCoroutine(getOriginatedContractsRoutine);
		}

		private void OnAccountDisconnected(AccountInfo _)
		{
			transferControls.SetActive(false);
		}

		private void GetContractTokenIds(string contractAddress)
		{
			var tokensForContractCoroutine = TezosManager.Instance.Tezos.API.GetTokensForContract(Callback, contractAddress, false, 10_000, new TokensForContractOrder.Default(0));
			StartCoroutine(tokensForContractCoroutine);
			return;

			void Callback(IEnumerable<Token> tokens)
			{
				// Join the token IDs with ", " as the separator
				var idsResult = string.Join(", ", tokens.Select(token => token.TokenId));
				tokenIdsText.text = idsResult;
			}
		}
	}

}