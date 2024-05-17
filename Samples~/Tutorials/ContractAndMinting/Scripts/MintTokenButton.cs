using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Helpers.Logging;
using TezosSDK.Samples.Tutorials.Common;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.Filters;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Models.Tokens;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace TezosSDK.Samples.Tutorials.ContractAndMinting
{

	public class MintTokenButton : MonoBehaviour
	{
		[SerializeField] private TMP_InputField tokensCountText;
		[SerializeField] private ContractInfoUI contractInfoUI;

		private void Start()
		{
			TezosManager.Instance.EventManager.WalletConnected += OnWalletConnected;
		}

		private void OnDestroy()
		{
			TezosManager.Instance.EventManager.WalletConnected -= OnWalletConnected;
		}

		private void OnWalletConnected(WalletInfo _)
		{
			GetTokensCount();
		}

		public void HandleMint()
		{
			TezosLog.Debug("Minting token...");

			var tokenMetadata = CreateRandomTokenMetadata();
			var destinationAddress = TezosManager.Instance.Tezos.WalletAccount.GetWalletAddress();
			var randomAmount = new Random().Next(1, 1024);

			TezosManager.Instance.Tezos.TokenContract.Mint(OnTokenMinted, tokenMetadata, destinationAddress,
				randomAmount);
		}

		private TokenMetadata CreateRandomTokenMetadata()
		{
			var randomInt = new Random().Next(1, int.MaxValue);

			// to preview: https://ipfs.io/ipfs/QmX4t8ikQgjvLdqTtL51v6iVun9tNE7y7Txiw4piGQVNgK
			const string _image_address = "ipfs://QmX4t8ikQgjvLdqTtL51v6iVun9tNE7y7Txiw4piGQVNgK";

			return new TokenMetadata
			{
				Name = $"testName_{randomInt}",
				Description = $"testDescription_{randomInt}",
				Symbol = $"TST_{randomInt}",
				Decimals = "0",
				DisplayUri = _image_address,
				ArtifactUri = _image_address,
				ThumbnailUri = _image_address
			};
		}

		private IEnumerator GetContractsRoutine()
		{
			TezosLog.Debug("No contract address found. Check originated contracts...");
			return TezosManager.Instance.Tezos.GetOriginatedContracts(OnContractsFetched);
		}

		private void GetTokensCount()
		{
			TezosLog.Debug("Getting tokens count...");

			StartCoroutine(string.IsNullOrEmpty(TezosManager.Instance.Tezos.TokenContract.Address)
				// if we don't have a contract address, get the originated (deployed) contracts
				? GetContractsRoutine()
				// otherwise, get the tokens for the deployed contract
				: GetTokensForContractRoutine());
		}

		private void GetTokensForContractResult(HttpResult<IEnumerable<Token>> result)
		{
			if (result.Success)
			{
				OnTokensFetched(result.Data);
			}
			else
			{
				TezosLog.Error(result.ErrorMessage);
			}
		}

		private IEnumerator GetTokensForContractRoutine()
		{
			TezosLog.Debug("Has contract, get tokens for it...");

			return TezosManager.Instance.Tezos.API.GetTokensForContract(GetTokensForContractResult,
				TezosManager.Instance.Tezos.TokenContract.Address, false, 10_000,
				new TokensForContractOrder.Default(0));
		}

		private void OnContractsFetched(HttpResult<IEnumerable<TokenContract>> result)
		{
			if (result.Success)
			{
				var contracts = result.Data.ToList();

				var allTokenContracts = contracts.ToList();

				if (!allTokenContracts.Any())
				{
					TezosLog.Debug("No contracts found");
					tokensCountText.text = "No contracts found.";
					return;
				}

				var contract = allTokenContracts.First();
				TezosLog.Debug($"Found {allTokenContracts.Count} contracts. Using {contract.Address}");
				TezosManager.Instance.Tezos.TokenContract = contract; // set the TokenContract on the Tezos instance

				contractInfoUI.SetAddress(contract.Address);
				StartCoroutine(GetTokensForContractRoutine());
			}
			else
			{
				TezosLog.Error(result.ErrorMessage);
			}
		}

		private void OnTokenMinted(TokenBalance tokenBalance)
		{
			TezosLog.Debug($"Successfully minted token with Token ID {tokenBalance.TokenId}");
			GetTokensCount();
		}

		private void OnTokensFetched(IEnumerable<Token> tokens)
		{
			TezosLog.Debug("Tokens fetched");

			var tokenList = tokens.ToList();

			TezosLog.Debug($"Found {tokenList.Count} tokens");
			tokensCountText.text = tokenList.Count.ToString();
		}
	}

}