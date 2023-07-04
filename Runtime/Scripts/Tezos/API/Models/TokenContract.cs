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
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("tokensCount")]
        public int TokensCount { get; set; }

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
                .GetActiveWalletAddress();

            // TezosSingleton
            //     .Instance
            //     .API
            //     .GetAccountCounter(counter =>
            //     {
            //         Debug.Log("Counter: " + counter);
            //     }, address);

            var script = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
            var code = JObject
                .Parse(script)
                .SelectToken("code");

            var michelineCode = Micheline.FromJson(code.ToString());
            var cs = new ContractScript(michelineCode);

            const string CONTRACT_ADDRESS = "KT1DTJEAte2SE1dTJNWS1qSck8pCmGpVpD6X";
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

                Debug.Log("TOKEN TRANSFERRED. HASH: " + transactionHash);
            }

            TezosSingleton
                .Instance
                .CallContract(
                    CONTRACT_ADDRESS,
                    entryPoint,
                    param);
        }

        public void Deploy(Action<string> contractAddressReceived)
        {
            var stringScript = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;

            var address = TezosSingleton
                .Instance
                .GetActiveWalletAddress();
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

                var transactionHash = JsonSerializer
                    .Deserialize<JsonElement>(response)
                    .GetProperty("transactionHash")
                    .ToString();

                CoroutineRunner.Instance.StartWrappedCoroutine(
                    TezosSingleton
                        .Instance
                        .API
                        .GetContractAddressByOperationHash(contractAddress =>
                        {
                            Address = contractAddress;
                            contractAddressReceived?.Invoke(contractAddress);
                        }, transactionHash));
            }

            TezosSingleton
                .Instance
                .OriginateContract(script);
        }
    }
}