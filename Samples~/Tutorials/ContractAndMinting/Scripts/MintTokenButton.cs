using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Samples.Tutorials.Common;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.Filters;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Models.Tokens;
using TMPro;
using UnityEngine;
using Random = System.Random;
using Logger = TezosSDK.Helpers.Logging.Logger;


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
			Logger.LogDebug("Minting token...");

			var tokenMetadata = CreateRandomTokenMetadata();
			var destinationAddress = TezosManager.Instance.WalletAccount.GetWalletAddress();
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
			Logger.LogDebug("No contract address found. Check originated contracts...");
			return TezosManager.Instance.Tezos.GetOriginatedContracts(OnContractsFetched);
		}

		private void GetTokensCount()
		{
			Logger.LogDebug("Getting tokens count...");

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
				Logger.LogError(result.ErrorMessage);
			}
		}

		private IEnumerator GetTokensForContractRoutine()
		{
			Logger.LogDebug("Has contract, get tokens for it...");

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
					Logger.LogDebug("No contracts found");
					tokensCountText.text = "No contracts found.";
					return;
				}

				var contract = allTokenContracts.First();
				Logger.LogDebug($"Found {allTokenContracts.Count} contracts. Using {contract.Address}");
				TezosManager.Instance.Tezos.TokenContract = contract; // set the TokenContract on the Tezos instance

				contractInfoUI.SetAddress(contract.Address);
				StartCoroutine(GetTokensForContractRoutine());
			}
			else
			{
				Logger.LogError(result.ErrorMessage);
			}
		}

		private void OnTokenMinted(TokenBalance tokenBalance)
		{
			Logger.LogDebug($"Successfully minted token with Token ID {tokenBalance.TokenId}");
			GetTokensCount();
		}

		private void OnTokensFetched(IEnumerable<Token> tokens)
		{
			Logger.LogDebug("Tokens fetched");

			var tokenList = tokens.ToList();

			Logger.LogDebug($"Found {tokenList.Count} tokens");
			tokensCountText.text = tokenList.Count.ToString();
		}
	}

}