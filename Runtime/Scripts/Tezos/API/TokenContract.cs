using System;
using System.Collections.Generic;
using System.Linq;
using Netezos.Contracts;
using Netezos.Encoding;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Helpers.HttpClients;
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

			CoroutineRunner.Instance.StartCoroutine(getContractTokens);

			return;

			void TokensReceived(Result<IEnumerable<Token>> result)
			{
				Logger.LogDebug("Got tokens for contract");

				if (result.Success)
				{
					var tokens = result.Data.ToList();
					var tokenId = tokens.Count;
					const string _entrypoint = "mint";

					var mintParameters = GetContractScript().BuildParameter(_entrypoint, new
					{
						address = destination,
						amount = amount.ToString(),
						metadata = tokenMetadata.GetMetadataDict(),
						token_id = tokenId.ToString()
					}).ToJson();

					_wallet.EventManager.ContractCallCompleted += MintCompleted; // TODO: This is not removed FIX IT
					_wallet.CallContract(Address, _entrypoint, mintParameters);
				}
				else
				{
					Logger.LogError(result.ErrorMessage);
				}
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
			Logger.LogDebug("Deploying contract...");
			_onDeployCompleted = completedCallback;

			var stringScript = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
			var address = _wallet.GetWalletAddress();
			var scriptWithAdmin = stringScript.Replace("CONTRACT_ADMIN", address);

			_wallet.EventManager.ContractCallCompleted += DeployCompleted; // TODO: This is not removed FIX IT

			_wallet.OriginateContract(scriptWithAdmin);
		}

		private void MintCompleted(OperationResult operationResult)
		{
			_wallet.EventManager.ContractCallCompleted -= MintCompleted; // TODO: This is not removed FIX IT

			var owner = _wallet.GetWalletAddress();

			var getOwnerTokensCoroutine = _tezosAPI.GetTokensForOwner(GetTokensCallback, owner, true, 10_000,
				new TokensForOwnerOrder.Default(0));

			CoroutineRunner.Instance.StartCoroutine(getOwnerTokensCoroutine);
		}

		private void GetTokensCallback(Result<IEnumerable<TokenBalance>> result)
		{
			if (result.Success)
			{
				_onMintCompleted.Invoke(result.Data.Last());
			}
			else
			{
				Logger.LogError(result.ErrorMessage);
			}
		}

		private void TransferCompleted(OperationResult operationResult)
		{
			var transactionHash = operationResult.TransactionHash;
			_onTransferCompleted.Invoke(transactionHash);
		}

		private void DeployCompleted(OperationResult operationResult)
		{
			Logger.LogDebug("Deploy completed");
			_wallet.EventManager.ContractCallCompleted -= DeployCompleted; // TODO: This is not removed FIX IT

			var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;
			var creator = _wallet.GetWalletAddress();

			CoroutineRunner.Instance.StartCoroutine(_tezosAPI.GetOriginatedContractsForOwner(OnGetContracts, creator,
				codeHash, 1000, new OriginatedContractsForOwnerOrder.Default(0)));

			return;

			void OnGetContracts(Result<IEnumerable<TokenContract>> result)
			{
				Logger.LogDebug("Got contracts for owner");

				if (result.Success)
				{
					var tokenContracts = result.Data.ToList();

					if (!tokenContracts.Any())
					{
						return;
					}

					var lastUsedContract = tokenContracts.Last();
					Address = lastUsedContract.Address;
					PlayerPrefs.SetString("CurrentContract:" + creator, lastUsedContract.Address);
					_onDeployCompleted.Invoke(lastUsedContract.Address);
				}
				else
				{
					Logger.LogError(result.ErrorMessage);
				}
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