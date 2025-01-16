using System.Linq;
using Tezos.API;
using Tezos.Logger;
using Tezos.Token;
using Tezos.WalletProvider;
using TezosSDK.Samples.Tutorials.Common;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace TezosSDK.Samples.Tutorials.ContractAndMinting
{

	public class MintTokenButton : MonoBehaviour
	{
		[SerializeField] private TMP_InputField tokensCountText;
		[SerializeField] private ContractInfoUI contractInfoUI;
		[SerializeField] private string contractAddress;
		
		private void Start()
		{
			TezosAPI.WalletConnected += OnWalletConnected;
			contractInfoUI.SetAddress(contractAddress);
		}

		private void OnDestroy()
		{
			TezosAPI.WalletConnected -= OnWalletConnected;
		}

		private void OnWalletConnected(WalletProviderData walletProviderData)
		{
			GetTokensCount();
		}

		public async void HandleMint()
		{
			//TezosLogger.LogDebug("Minting token...");

			var tokenMetadata = CreateRandomTokenMetadata();
			var destinationAddress = TezosAPI.GetConnectionAddress();
			var randomAmount = new Random().Next(1, 1024);
			
			var tokenBalance = await TezosAPI.Mint(tokenMetadata, destinationAddress, randomAmount, contractAddress);
			TezosLogger.LogDebug($"Successfully minted token with Token ID {tokenBalance.TokenId}");
			GetTokensCount();
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

		private async void GetTokensCount()
		{
			var res = await TezosAPI.GetTokensForContract(contractAddress, false, 10_000, new TokensForContractOrder.Default(0));
			tokensCountText.text = res.Count().ToString();
		}

	}

}