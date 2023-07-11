using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Netezos.Contracts;
using Netezos.Encoding;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers;
using TezosSDK.Tezos.API.Models.Abstract;
using TezosSDK.Tezos.API.Models.Tokens;
using UnityEngine;
using TezosSDK.Tezos.API.Models.Filters;


namespace TezosSDK.Tezos.API.Models
{
    public class TokenContract : IFA2
    {
        public string Address { get; set; }
        public int TokensCount { get; set; }
        public DateTime LastActivityTime { get; set; }

        private Action<TokenBalance> OnMintCompleted;
        private Action<string> OnTransferCompleted;
        private Action<string> OnDeployCompleted;

        public TokenContract(string address)
        {
            Address = address;
        }

        public TokenContract()
        {
        }

        public void Mint(
            Action<TokenBalance> completedCallback,
            TokenMetadata tokenMetadata,
            string destination,
            int amount)
        {
            OnMintCompleted = completedCallback;

            var getContractTokens = TezosSingleton
                .Instance
                .API
                .GetTokensForContract(
                    callback: TokensReceived,
                    contractAddress: Address,
                    withMetadata: false,
                    maxItems: 10_000,
                    orderBy: new TokensForContractOrder.Default(0));

            CoroutineRunner.Instance.StartWrappedCoroutine(getContractTokens);

            void TokensReceived(IEnumerable<Token> tokens)
            {
                var tokenId = tokens?.Count() ?? 0;
                const string entrypoint = "mint";

                var mintParameters = GetContractScript().BuildParameter(
                        entrypoint: entrypoint,
                        value: new
                        {
                            address = destination,
                            amount = amount.ToString(),
                            metadata = tokenMetadata.GetMetadataDict(),
                            token_id = tokenId.ToString()
                        })
                    .ToJson();

                TezosSingleton
                    .Instance
                    .Wallet
                    .MessageReceiver
                    .ContractCallCompleted += MintCompleted;

                TezosSingleton
                    .Instance
                    .Wallet
                    .CallContract(
                        contractAddress: Address,
                        entryPoint: entrypoint,
                        input: mintParameters);
            }
        }

        private void MintCompleted(string response)
        {
            var owner = TezosSingleton
                .Instance
                .Wallet
                .GetActiveAddress();

            var getOwnerTokensCoroutine = TezosSingleton
                .Instance
                .API
                .GetTokensForOwner(
                    callback: tokens => { OnMintCompleted.Invoke(tokens.Last()); },
                    owner,
                    withMetadata: true,
                    maxItems: 10_000,
                    orderBy: new TokensForOwnerOrder.Default(0));

            CoroutineRunner.Instance.StartWrappedCoroutine(getOwnerTokensCoroutine);
        }

        public void Transfer(
            Action<string> completedCallback,
            string destination,
            int tokenId,
            int amount)
        {
            OnTransferCompleted = completedCallback;

            var activeAddress = TezosSingleton
                .Instance
                .Wallet
                .GetActiveAddress();

            const string entryPoint = "transfer";

            var param = GetContractScript().BuildParameter(
                entrypoint: entryPoint,
                value: new List<object>
                {
                    new
                    {
                        from_ = activeAddress,
                        txs = new List<object>
                        {
                            new
                            {
                                to_ = destination,
                                token_id = tokenId,
                                amount
                            }
                        }
                    }
                }).ToJson();

            TezosSingleton
                .Instance
                .Wallet
                .MessageReceiver
                .ContractCallCompleted += TransferCompleted;

            TezosSingleton
                .Instance
                .Wallet
                .CallContract(
                    contractAddress: Address,
                    entryPoint: entryPoint,
                    input: param);
        }

        private void TransferCompleted(string response)
        {
            var transactionHash = JsonSerializer
                .Deserialize<JsonElement>(response)
                .GetProperty("transactionHash")
                .ToString();

            OnTransferCompleted.Invoke(transactionHash);
        }

        public void Deploy(Action<string> completedCallback)
        {
            OnDeployCompleted = completedCallback;

            var stringScript = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
            var address = TezosSingleton
                .Instance
                .Wallet
                .GetActiveAddress();
            var scriptWithAdmin = stringScript.Replace("CONTRACT_ADMIN", address);

            TezosSingleton
                .Instance
                .Wallet
                .MessageReceiver
                .ContractCallCompleted += DeployCompleted;

            TezosSingleton
                .Instance
                .Wallet
                .OriginateContract(scriptWithAdmin);
        }

        private void DeployCompleted(string response)
        {
            var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;
            var creator = TezosSingleton
                .Instance
                .Wallet
                .GetActiveAddress();

            CoroutineRunner.Instance.StartWrappedCoroutine(
                TezosSingleton
                    .Instance
                    .API
                    .GetOriginatedContractsForOwner(contracts =>
                        {
                            var tokenContracts = contracts.ToList();
                            if (!tokenContracts.Any()) return;

                            var lastUsedContract = tokenContracts.Last();
                            Address = lastUsedContract.Address;
                            PlayerPrefs.SetString("CurrentContract:" + creator, lastUsedContract.Address);
                            OnDeployCompleted.Invoke(lastUsedContract.Address);
                        },
                        creator,
                        codeHash,
                        maxItems: 1000,
                        orderBy: new OriginatedContractsForOwnerOrder.Default(0)));
        }

        private ContractScript GetContractScript()
        {
            var script = Resources
                .Load<TextAsset>("Contracts/FA2TokenContract")
                .text;

            var code = JObject
                .Parse(script)
                .SelectToken("code");

            return new ContractScript(Micheline.FromJson(code.ToString()));
        }
    }
}