using System.Collections.Generic;
using System.Linq;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using TezosSDK.Tutorials.Common;
using TMPro;
using UnityEngine;

namespace TezosSDK.Tutorials.TransferToken
{

	public class TransferTokenHandler : MonoBehaviour
	{
		[SerializeField] private TMP_InputField availableTokensTMP;
		[SerializeField] private ContractInfoUI contractInfoUI;

		private void Start()
		{
			// Subscribe to account connection events
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
		}

		private void OnDestroy()
		{
			TezosManager.Instance.EventManager.WalletConnected -= OnWalletConnected;
		}

		private void OnWalletConnected(WalletInfo _)
		{
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
					var activeAddress = TezosManager.Instance.Tezos.Wallet.GetWalletAddress();

					availableTokensTMP.text = $"{activeAddress} didn't deploy any contract yet.";
					return;
				}

				var initializedContract = tokenContracts.First();
				TezosManager.Instance.Tezos.TokenContract = initializedContract;

				contractInfoUI.SetAddress(initializedContract.Address);
				GetContractTokenIds(initializedContract.Address);
			});

			StartCoroutine(getOriginatedContractsRoutine);
		}

		private void GetContractTokenIds(string contractAddress)
		{
			var tokensForContractCoroutine = TezosManager.Instance.Tezos.API.GetTokensForContract(Callback,
				contractAddress, false, 10_000, new TokensForContractOrder.Default(0));

			StartCoroutine(tokensForContractCoroutine);
			return;

			void Callback(IEnumerable<Token> tokens)
			{
				// Join the token IDs with ", " as the separator
				var idsResult = string.Join(", ", tokens.Select(token => token.TokenId));
				availableTokensTMP.text = idsResult;
			}
		}
	}

}