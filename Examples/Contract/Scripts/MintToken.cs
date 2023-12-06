using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TezosSDK.Beacon;
using TezosSDK.Common.Scripts;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using TMPro;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;
using Random = System.Random;

namespace TezosSDK.Contract.Scripts
{

	public class MintToken : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI tokensCountText;
		[SerializeField] private ContractInfoUI contractInfoUI;

		private void Start()
		{
			var activeAddress = TezosManager.Instance.Wallet.GetActiveAddress();

			// if there is no active address, subscribe to account connection events
			if (string.IsNullOrEmpty(activeAddress))
			{
				TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
			}
			else // otherwise, get the tokens count
			{
				GetTokensCount();
			}
		}

		private void OnAccountConnected(AccountInfo _)
		{
			GetTokensCount();
		}

		public void HandleMint()
		{
			var tokenMetadata = CreateRandomTokenMetadata();
			var destinationAddress = TezosManager.Instance.Wallet.GetActiveAddress();
			var randomAmount = new Random().Next(1, 1024);

			TezosManager.Instance.Tezos.TokenContract.Mint(OnTokenMinted, tokenMetadata, destinationAddress, randomAmount);
		}

		private TokenMetadata CreateRandomTokenMetadata()
		{
			var randomInt = new Random().Next(1, int.MaxValue);
			const string imageAddress = "ipfs://QmX4t8ikQgjvLdqTtL51v6iVun9tNE7y7Txiw4piGQVNgK";

			return new TokenMetadata
			{
				Name = $"testName_{randomInt}",
				Description = $"testDescription_{randomInt}",
				Symbol = $"TST_{randomInt}",
				Decimals = "0",
				DisplayUri = imageAddress,
				ArtifactUri = imageAddress,
				ThumbnailUri = imageAddress
			};
		}

		private void GetTokensCount()
		{
			StartCoroutine(string.IsNullOrEmpty(TezosManager.Instance.Tezos.TokenContract.Address)
				? GetOriginatedContractsRoutine()
				: GetTokensForContractRoutine());
		}

		private IEnumerator GetTokensForContractRoutine()
		{
			return TezosManager
				.Instance
				.Tezos
				.API
				.GetTokensForContract(
					OnTokensFetched,
					TezosManager.Instance.Tezos.TokenContract.Address,
					false,
					10_000,
					new TokensForContractOrder.Default(0));
		}

		private IEnumerator GetOriginatedContractsRoutine()
		{
			return TezosManager
				.Instance
				.Tezos
				.GetOriginatedContracts(contracts =>
				{
					var tokenContracts = contracts.ToList();
					if (!tokenContracts.Any())
					{
						var activeAddress = TezosManager
							.Instance
							.Tezos
							.Wallet
							.GetActiveAddress();

						tokensCountText.text = $"{activeAddress} didn't deploy any contract yet.";
						return;
					}

					var initializedContract = tokenContracts.First();
					TezosManager
						.Instance
						.Tezos
						.TokenContract = initializedContract;
					
					contractInfoUI.SetAddress(initializedContract.Address);

					StartCoroutine(GetTokensForContractRoutine());
				});
		}

		private void OnTokenMinted(TokenBalance tokenBalance)
		{
			Logger.LogDebug($"Successfully minted token with Token ID {tokenBalance.TokenId}");
			GetTokensCount();
		}

		private void OnTokensFetched(IEnumerable<Token> tokens)
		{
			tokensCountText.text = tokens.Count().ToString();
		}
	}

}