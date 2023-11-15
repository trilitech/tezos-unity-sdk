using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Netezos.Contracts;
using Netezos.Encoding;
using Newtonsoft.Json.Linq;
using TezosSDK.Beacon;
using TezosSDK.Helpers;
using TezosSDK.Tezos.API.Models.Abstract;
using TezosSDK.Tezos.API.Models.Tokens;
using UnityEngine;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.Wallet;


namespace TezosSDK.Tezos.API.Models
{
    public class TokenContract : IFA2
    {
        public string Address { get; set; }
        public int TokensCount { get; set; }
        public DateTime LastActivityTime { get; set; }

        private Action<TokenBalance> _onMintCompleted;
        private Action<string> _onTransferCompleted;
        private Action<string> _onDeployCompleted;

        private readonly IWalletProvider _wallet;
        private readonly ITezosAPI _tezosAPI;

        public TokenContract(string address)
        {
            _wallet = TezosManager.Instance.Wallet;
            _tezosAPI = TezosManager.Instance.Tezos.API;
            Address = address;
        }

        public TokenContract()
        {
            _wallet = TezosManager.Instance.Wallet;
            _tezosAPI = TezosManager.Instance.Tezos.API;
        }

        public void Mint(
            Action<TokenBalance> completedCallback,
            TokenMetadata tokenMetadata,
            string destination,
            int amount)
        {
            _onMintCompleted = completedCallback;

            var getContractTokens = _tezosAPI
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

                _wallet
                    .EventManager
                    .ContractCallCompleted += MintCompleted;

                _wallet.CallContract(
                    contractAddress: Address,
                    entryPoint: entrypoint,
                    input: mintParameters);
            }
        }

        private void MintCompleted(OperationResult operation_result)
        {
            var owner = _wallet.GetActiveAddress();

            var getOwnerTokensCoroutine = _tezosAPI
                .GetTokensForOwner(
                    callback: tokens => { _onMintCompleted.Invoke(tokens.Last()); },
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
            _onTransferCompleted = completedCallback;

            var activeAddress = _wallet.GetActiveAddress();

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

            _wallet
                .EventManager
                .ContractCallCompleted += TransferCompleted;

            _wallet.CallContract(
                contractAddress: Address,
                entryPoint: entryPoint,
                input: param);
        }

        private void TransferCompleted(OperationResult operation_result)
        {
            var transactionHash = operation_result.TransactionHash;
            _onTransferCompleted.Invoke(transactionHash);
        }

        public void Deploy(Action<string> completedCallback)
        {
            _onDeployCompleted = completedCallback;

            var stringScript = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
            var address = _wallet.GetActiveAddress();
            var scriptWithAdmin = stringScript.Replace("CONTRACT_ADMIN", address);

            _wallet
                .EventManager
                .ContractCallCompleted += DeployCompleted;

            _wallet.OriginateContract(scriptWithAdmin);
        }

        private void DeployCompleted(OperationResult operation_result)
        {
            var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;
            var creator = _wallet.GetActiveAddress();

            CoroutineRunner.Instance.StartWrappedCoroutine(
                _tezosAPI.GetOriginatedContractsForOwner(contracts =>
                    {
                        var tokenContracts = contracts.ToList();
                        if (!tokenContracts.Any()) return;

                        var lastUsedContract = tokenContracts.Last();
                        Address = lastUsedContract.Address;
                        PlayerPrefs.SetString("CurrentContract:" + creator, lastUsedContract.Address);
                        _onDeployCompleted.Invoke(lastUsedContract.Address);
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