using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using JetBrains.Annotations;
using Netezos.Contracts;
using Netezos.Encoding;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers;
using TezosSDK.Tezos.API.Models.Abstract;
using UnityEngine;

namespace TezosSDK.Tezos.API.Models
{
    public class TokenContract : IFA2
    {
        [CanBeNull] public string Address { get; set; }

        public TokenContract(string address)
        {
            Address = address;
        }

        public TokenContract()
        {
        }

        public void Mint()
        {
            throw new NotImplementedException();
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
            
            var currentDir = Utils.GetThisFileDir();
            var script = File
                .ReadAllText($@"{currentDir}/../../../Contracts/FA2Token.json");
            
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
            var currentDir = Utils.GetThisFileDir();
            var stringScript = File
                .ReadAllText($@"{currentDir}/../../../Contracts/FA2Token.json");

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