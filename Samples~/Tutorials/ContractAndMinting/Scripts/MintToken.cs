using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using TezosSDK.Tutorials.Common;
using TMPro;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;
using Random = System.Random;

namespace TezosSDK.Tutorials.ContractAndMinting
{

	public class MintToken : MonoBehaviour
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
			var tokenMetadata = CreateRandomTokenMetadata();
			var destinationAddress = TezosManager.Instance.Wallet.GetWalletAddress();
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
			Logger.LogDebug("No contract address found. Checking originated contracts...");
			return TezosManager.Instance.Tezos.GetOriginatedContracts(OnContractsFetched);
		}

		private void GetTokensCount()
		{
			StartCoroutine(string.IsNullOrEmpty(TezosManager.Instance.Tezos.TokenContract.Address)
				// if we don't have a contract address, get the originated (deployed) contracts
				? GetContractsRoutine()
				// otherwise, get the tokens for the deployed contract
				: GetTokensForContractRoutine());
		}

		private IEnumerator GetTokensForContractRoutine()
		{
			return TezosManager.Instance.Tezos.API.GetTokensForContract(OnTokensFetched,
				TezosManager.Instance.Tezos.TokenContract.Address, false, 10_000,
				new TokensForContractOrder.Default(0));
		}

		private void OnContractsFetched(IEnumerable<TokenContract> contracts)
		{
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

		private void OnTokenMinted(TokenBalance tokenBalance)
		{
			Logger.LogDebug($"Successfully minted token with Token ID {tokenBalance.TokenId}");
			GetTokensCount();
		}

		private void OnTokensFetched(IEnumerable<Token> tokens)
		{
			var tokenList = tokens.ToList();

			Logger.LogDebug($"Found {tokenList.Count} tokens");
			tokensCountText.text = tokenList.Count.ToString();
		}
	}

}