using System.Linq;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using TMPro;
using UnityEngine;
using Random = System.Random;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Contract.Scripts
{
    public class MintToken : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tokensCountText;

        private void Start()
        {
            var activeAddress = TezosManager
                .Instance
                .Wallet
                .GetActiveAddress();

            if (string.IsNullOrEmpty(activeAddress))
            {
                TezosManager
                    .Instance
                    .MessageReceiver
                    .AccountConnected += _ => GetTokensCount();
            }
            else
            {
                GetTokensCount();
            }
        }

        public void HandleMint()
        {
            var rnd = new Random();
            var randomInt = rnd.Next(1, int.MaxValue);
            var randomAmount = rnd.Next(1, 1024);

            var destinationAddress = TezosManager
                .Instance
                .Wallet
                .GetActiveAddress();

            const string imageAddress = "ipfs://QmX4t8ikQgjvLdqTtL51v6iVun9tNE7y7Txiw4piGQVNgK";

            var tokenMetadata = new TokenMetadata
            {
                Name = $"testName_{randomInt}",
                Description = $"testDescription_{randomInt}",
                Symbol = $"TST_{randomInt}",
                Decimals = "0",
                DisplayUri = imageAddress,
                ArtifactUri = imageAddress,
                ThumbnailUri = imageAddress
            };

            TezosManager
                .Instance
                .Tezos
                .TokenContract
                .Mint(
                    completedCallback: OnTokenMinted,
                    tokenMetadata: tokenMetadata,
                    destination: destinationAddress,
                    amount: randomAmount);
        }

        private void OnTokenMinted(TokenBalance tokenBalance)
        {
            Logger.LogDebug($"Successfully minted token with Token ID {tokenBalance.TokenId}");
            GetTokensCount();
        }

        private void GetTokensCount()
        {
            var contractAddress = TezosManager
                .Instance
                .Tezos
                .TokenContract
                .Address;

            if (string.IsNullOrEmpty(contractAddress)) return;

            var getOwnerTokensCoroutine = TezosManager
                .Instance
                .Tezos
                .API
                .GetTokensForContract(
                    callback: tokens =>
                    {
                        tokensCountText.text = tokens
                            .Count()
                            .ToString();
                    },
                    contractAddress: contractAddress,
                    withMetadata: false,
                    maxItems: 10_000,
                    orderBy: new TokensForContractOrder.Default(0));

            StartCoroutine(getOwnerTokensCoroutine);
        }
    }
}