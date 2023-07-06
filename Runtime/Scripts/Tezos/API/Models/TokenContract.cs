using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        [JsonPropertyName("address")] public string Address { get; set; }

        [JsonPropertyName("tokensCount")] public int TokensCount { get; set; }

        [JsonPropertyName("lastActivityTime")] public DateTime LastActivityTime { get; set; }

        public TokenContract(string address)
        {
            Address = address;
        }

        public TokenContract()
        {
        }

        public void Mint(Action<string> callback,
            TokenMetadata tokenMetadata,
            string destination,
            int amount)
        {
            var getTokenIdRoutine = TezosSingleton
                .Instance
                .API
                .GetTokensForContract(
                    callback: tokens => { TokensCountReceived(tokens.Count()); },
                    contractAddress: Address,
                    withMetadata: false,
                    maxItems: 10_000,
                    orderBy: new TokensForContractOrder.Default(0));

            CoroutineRunner.Instance.StartWrappedCoroutine(getTokenIdRoutine);

            void TokensCountReceived(int tokensCount)
            {
                var script = Resources
                    .Load<TextAsset>("Contracts/FA2TokenContract")
                    .text;

                var code = JObject
                    .Parse(script)
                    .SelectToken("code");

                var michelineCode = Micheline.FromJson(code!.ToString());
                var cs = new ContractScript(michelineCode!);

                var mintParameters = cs.BuildParameter(
                        "mint",
                        new
                        {
                            address = destination,
                            amount = amount.ToString(),
                            metadata = tokenMetadata.GetMetadataDict(),
                            token_id = tokensCount.ToString()
                        })
                    .ToJson();

                TezosSingleton
                    .Instance
                    .Wallet
                    .MessageReceiver
                    .ContractCallCompleted += ContractCallCompleted;

                void ContractCallCompleted(string response)
                {
                    TezosSingleton
                        .Instance
                        .Wallet
                        .MessageReceiver
                        .ContractCallCompleted -= ContractCallCompleted;

                    var transactionHash = JsonSerializer
                        .Deserialize<JsonElement>(response)
                        .GetProperty("transactionHash")
                        .ToString();

                    callback.Invoke(transactionHash);
                }

                TezosSingleton
                    .Instance
                    .Wallet
                    .CallContract(
                        Address,
                        "mint",
                        mintParameters);
            }
        }

        public void Transfer(Action<string> transactionHash)
        {
            var address = TezosSingleton
                .Instance
                .Wallet
                .GetActiveAddress();
            var script = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
            var code = JObject
                .Parse(script)
                .SelectToken("code");

            var michelineCode = Micheline.FromJson(code.ToString());
            var cs = new ContractScript(michelineCode);
            
            const string entryPoint = "transfer";

            var param = cs.BuildParameter(
                "transfer",
                new List<object>
                {
                    new
                    {
                        from_ = address,
                        txs = new List<object>
                        {
                            new
                            {
                                // todo: get from UI
                                to_ = "tz1Z7tMm4kwdXWhxLVcPr1ZyvFdCH6MnfNYN",
                                token_id = 0,
                                amount = "11"
                            }
                        }
                    }
                }).ToJson();

            TezosSingleton
                .Instance
                .Wallet
                .MessageReceiver
                .ContractCallCompleted += ContractCallCompleted;

            void ContractCallCompleted(string response)
            {
                TezosSingleton
                    .Instance
                    .Wallet
                    .MessageReceiver
                    .ContractCallCompleted -= ContractCallCompleted;

                var transactionHash = JsonSerializer
                    .Deserialize<JsonElement>(response)
                    .GetProperty("transactionHash")
                    .ToString();

                Debug.Log($"TOKEN TRANSFERRED. HASH: {transactionHash}");
            }

            TezosSingleton
                .Instance
                .Wallet
                .CallContract(
                    Address,
                    entryPoint,
                    param);
        }

        public void Deploy(Action<string> deployedContractAddress)
        {
            var stringScript = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
            var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;

            var address = TezosSingleton
                .Instance
                .Wallet
                .GetActiveAddress();

            var script = stringScript.Replace("CONTRACT_ADMIN", address);

            // todo: refactor this
            TezosSingleton
                .Instance
                .Wallet
                .MessageReceiver
                .ContractCallCompleted -= DeployCompleted;

            TezosSingleton
                .Instance
                .Wallet
                .MessageReceiver
                .ContractCallCompleted += DeployCompleted;

            void DeployCompleted(string response)
            {
                TezosSingleton
                    .Instance
                    .Wallet
                    .MessageReceiver
                    .ContractCallCompleted -= DeployCompleted;

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
                                PlayerPrefs.SetString("CurrentContract:" + TezosSingleton.Instance.Wallet.GetActiveAddress(), lastUsedContract.Address);
                                deployedContractAddress?.Invoke(lastUsedContract.Address);
                            },
                            creator: address,
                            codeHash: codeHash,
                            maxItems: 1000,
                            new OriginatedContractsForOwnerOrder.Default(0)));
            }

            TezosSingleton
                .Instance
                .Wallet
                .OriginateContract(script);
        }
    }
}