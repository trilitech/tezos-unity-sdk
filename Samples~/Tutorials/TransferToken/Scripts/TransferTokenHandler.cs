using System.Linq;
using Tezos.API;
using Tezos.Logger;
using Tezos.WalletProvider;
using TezosSDK.Samples.Tutorials.Common;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.TransferToken
{

	public class TransferTokenHandler : MonoBehaviour
	{
		[SerializeField] private TMP_InputField availableTokensTMP;
		[SerializeField] private ContractInfoUI contractInfoUI;
		[SerializeField] private string contractAddress;
		

		private void Start()
		{
			// Subscribe to account connection events
			TezosAPI.WalletConnected += OnWalletConnected;
		}

		private void OnDestroy()
		{
			TezosAPI.WalletConnected -= OnWalletConnected;
		}

		private async void OnWalletConnected(WalletProviderData walletProviderData)
		{
			if (!string.IsNullOrEmpty(contractAddress))
			{
				GetContractTokenIds(contractAddress);
				return;
			}
			
			var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text; // TODO: How to get the code hash for other contracts?
			var creator = TezosAPI.GetConnectionAddress();
			
			var result = await TezosAPI.GetOriginatedContractsForOwner(creator, codeHash, 5, new OriginatedContractsForOwnerOrder.Default(0));
			var tokenContracts = result.ToList();
			
			if (!tokenContracts.Any())
			{
				availableTokensTMP.text =
					$"{creator} didn't deploy any contract yet.";

				return;
			}

			var initializedContract = tokenContracts.First();

			contractInfoUI.SetAddress(initializedContract.Address);
			GetContractTokenIds(initializedContract.Address);
		}

		private async void GetContractTokenIds(string addr)
		{
			TezosLogger.LogDebug($"Getting token IDs for contract: {addr}");
			
			var tokenData = await TezosAPI.GetTokensForContract(addr, false, 10_000, new TokensForContractOrder.Default(0));

			var tokens = tokenData.ToList();

			var idsResult = string.Join(", ", tokens.Select(token => token.TokenId));
			availableTokensTMP.text = idsResult;
		}
	}

}