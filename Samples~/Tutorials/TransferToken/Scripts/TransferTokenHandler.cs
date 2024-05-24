using System.Collections.Generic;
using System.Linq;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Helpers.Logging;
using TezosSDK.Samples.Tutorials.Common;
using TezosSDK.Tezos.Filters;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Models.Tokens;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.TransferToken
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

			var getOriginatedContractsRoutine = TezosManager.Instance.Tezos.GetOriginatedContracts(result =>
			{
				if (!result.Success)
				{
					TezosLogger.LogError($"Failed to get originated contracts: {result.ErrorMessage}");
					return;
				}

				var tokenContracts = result.Data.ToList();

				if (!tokenContracts.Any())
				{
					availableTokensTMP.text =
						$"{TezosManager.Instance.Tezos.WalletAccount.GetWalletAddress()} didn't deploy any contract yet.";

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
			TezosLogger.LogDebug($"Getting token IDs for contract: {contractAddress}");

			var tokensForContractCoroutine = TezosManager.Instance.Tezos.API.GetTokensForContract(Callback,
				contractAddress, false, 10_000, new TokensForContractOrder.Default(0));

			StartCoroutine(tokensForContractCoroutine);
			return;

			void Callback(HttpResult<IEnumerable<Token>> result)
			{
				var tokens = result.Data.ToList();
				TezosLogger.LogDebug($"Received {tokens.Count()} tokens for contract: {contractAddress}");
				// Join the token IDs with ", " as the separator
				var idsResult = string.Join(", ", tokens.Select(token => token.TokenId));
				availableTokensTMP.text = idsResult;
			}
		}
	}

}