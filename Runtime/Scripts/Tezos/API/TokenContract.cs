using System;
using System.Collections.Generic;
using System.Linq;
using Netezos.Contracts;
using Netezos.Encoding;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos.API
{

	public class TokenContract : IFa2
	{
		private readonly ITezosAPI _tezosAPI;
		private readonly IWalletProvider _wallet;

		private Action<string> _onDeployCompleted;
		private Action<TokenBalance> _onMintCompleted;
		private Action<string> _onTransferCompleted;

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

		public string Address { get; set; }
		public int TokensCount { get; set; }
		public DateTime LastActivityTime { get; set; }

		public void Mint(
			Action<TokenBalance> completedCallback,
			TokenMetadata tokenMetadata,
			string destination,
			int amount)
		{
			_onMintCompleted = completedCallback;

			Logger.LogDebug($"Minting {amount} tokens to {destination} with metadata {tokenMetadata}");

			var getContractTokens = _tezosAPI.GetTokensForContract(TokensReceived, Address, false, 10_000,
				new TokensForContractOrder.Default(0));

			CoroutineRunner.Instance.StartWrappedCoroutine(getContractTokens);

			return;

			void TokensReceived(IEnumerable<Token> tokens)
			{
				var tokenId = tokens?.Count() ?? 0;
				const string _entrypoint = "mint";

				var mintParameters = GetContractScript().BuildParameter(_entrypoint, new
				{
					address = destination,
					amount = amount.ToString(),
					metadata = tokenMetadata.GetMetadataDict(),
					token_id = tokenId.ToString()
				}).ToJson();

				_wallet.EventManager.ContractCallCompleted += MintCompleted;
				_wallet.CallContract(Address, _entrypoint, mintParameters);
			}
		}

		public void Transfer(Action<string> completedCallback, string destination, int tokenId, int amount)
		{
			_onTransferCompleted = completedCallback;
			var activeAddress = _wallet.GetWalletAddress();
			const string _entry_point = "transfer";

			var param = GetContractScript().BuildParameter(_entry_point, new List<object>
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

			_wallet.EventManager.ContractCallCompleted += TransferCompleted;
			_wallet.CallContract(Address, _entry_point, param);
		}

		public void Deploy(Action<string> completedCallback)
		{
			_onDeployCompleted = completedCallback;

			var stringScript = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
			var address = _wallet.GetWalletAddress();
			var scriptWithAdmin = stringScript.Replace("CONTRACT_ADMIN", address);

			_wallet.EventManager.ContractCallCompleted += DeployCompleted;

			_wallet.OriginateContract(scriptWithAdmin);
		}

		private void MintCompleted(OperationResult operationResult)
		{
			var owner = _wallet.GetWalletAddress();

			var getOwnerTokensCoroutine = _tezosAPI.GetTokensForOwner(GetTokensCallback, owner, true, 10_000,
				new TokensForOwnerOrder.Default(0));

			CoroutineRunner.Instance.StartWrappedCoroutine(getOwnerTokensCoroutine);
		}

		private void GetTokensCallback(IEnumerable<TokenBalance> tokens)
		{
			_onMintCompleted.Invoke(tokens.Last());
		}

		private void TransferCompleted(OperationResult operationResult)
		{
			var transactionHash = operationResult.TransactionHash;
			_onTransferCompleted.Invoke(transactionHash);
		}

		private void DeployCompleted(OperationResult operationResult)
		{
			var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;
			var creator = _wallet.GetWalletAddress();

			CoroutineRunner.Instance.StartWrappedCoroutine(_tezosAPI.GetOriginatedContractsForOwner(OnGetContracts,
				creator, codeHash, 1000, new OriginatedContractsForOwnerOrder.Default(0)));

			return;

			void OnGetContracts(IEnumerable<TokenContract> contracts)
			{
				var tokenContracts = contracts.ToList();

				if (!tokenContracts.Any())
				{
					return;
				}

				var lastUsedContract = tokenContracts.Last();
				Address = lastUsedContract.Address;
				PlayerPrefs.SetString("CurrentContract:" + creator, lastUsedContract.Address);
				_onDeployCompleted.Invoke(lastUsedContract.Address);
			}
		}

		private ContractScript GetContractScript()
		{
			var script = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
			var code = JObject.Parse(script).SelectToken("code");

			return new ContractScript(Micheline.FromJson(code.ToString()));
		}
	}

}