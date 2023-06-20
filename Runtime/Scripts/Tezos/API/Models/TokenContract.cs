using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Netezos.Encoding;
using TezosSDK.Helpers;
using TezosSDK.Tezos.API.Models.Abstract;

namespace TezosSDK.Tezos.API.Models
{
    public class TokenContract : IFA2
    {
        [CanBeNull] public string Address { get; private set; }

        public TokenContract(string address)
        {
            Address = address;
        }

        public TokenContract()
        {
        }

        public Task<string> Mint()
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Transfer()
        {
            throw new System.NotImplementedException();
        }

        public void Deploy(Action<string> contractAddressReceived)
        {
            var currentDir = Utils.GetThisFileDir();
            var stringScript = File
                .ReadAllText($@"{currentDir}/../../../Contracts/FA2Token.json");
            
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
                .OriginateContract(stringScript);
        }
    }
}