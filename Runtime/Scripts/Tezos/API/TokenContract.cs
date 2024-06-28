using System;
using System.Collections.Generic;
using System.Linq;
using Netezos.Contracts;
using Netezos.Encoding;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Filters;
using TezosSDK.Tezos.Interfaces.API;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Models.Tokens;
using UnityEngine;

namespace TezosSDK.Tezos.API
{

	public class TokenContract : IFa2
	{
		private readonly ITezosAPI _tezosAPI;
		private readonly IWalletAccount _walletAccount;
		private readonly IWalletContract _walletContract;
		private readonly IWalletTransaction _walletTransaction;

		private Action<string> _onDeployCompleted;
		private Action<TokenBalance> _onMintCompleted;
		private Action<string> _onTransferCompleted;

		public TokenContract(
			string address,
			IWalletAccount walletAccount,
			IWalletTransaction walletTransaction,
			IWalletContract walletContract,
			ITezosAPI tezosAPI)
		{
			Address = address;
			_walletAccount = walletAccount;
			_walletTransaction = walletTransaction;
			_walletContract = walletContract;
			_tezosAPI = tezosAPI;
		}

		// Constructor without address parameter
		public TokenContract(
			IWalletAccount walletAccount,
			IWalletTransaction walletTransaction,
			IWalletContract walletContract,
			ITezosAPI tezosAPI) : this(null, walletAccount, walletTransaction, walletContract,
			tezosAPI)
		{
		}
		
		// Parameterless constructor for serialization
		public TokenContract()
		{
			_walletAccount = TezosManager.Instance.Tezos.WalletAccount;
			_walletTransaction = TezosManager.Instance.Tezos.WalletTransaction;
			_walletContract = TezosManager.Instance.Tezos.WalletContract;
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

			TezosLogger.LogDebug($"Minting {amount} tokens to {destination} with metadata {tokenMetadata}");

			var getContractTokens = _tezosAPI.GetTokensForContract(TokensReceived, Address, false, 10_000,
				new TokensForContractOrder.Default(0));

			CoroutineRunner.Instance.StartCoroutine(getContractTokens);

			return;

			void TokensReceived(HttpResult<IEnumerable<Token>> result)
			{
				TezosLogger.LogDebug("Got tokens for contract");

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

					TezosManager.Instance.EventManager.OperationCompleted += MintCompleted; 

					_walletTransaction.CallContract(Address, _entrypoint, mintParameters);
				}
				else
				{
					TezosLogger.LogError(result.ErrorMessage);
				}
			}
		}

		public void Transfer(Action<string> completedCallback, string destination, int tokenId, int amount)
		{
			_onTransferCompleted = completedCallback;
			var activeAddress = _walletAccount.GetWalletAddress();
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

			TezosManager.Instance.EventManager.OperationCompleted += TransferCompleted;
			_walletTransaction.CallContract(Address, _entry_point, param);
		}

		public void Deploy(Action<string> completedCallback)
		{
			TezosLogger.LogDebug("Deploying contract...");
			_onDeployCompleted = completedCallback;

			var stringScript = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
			var address = _walletAccount.GetWalletAddress();
			var scriptWithAdmin = stringScript.Replace("CONTRACT_ADMIN", address);

			TezosManager.Instance.EventManager.OperationCompleted += DeployCompleted;

			_walletContract.OriginateContract(scriptWithAdmin);
		}

		private void MintCompleted(OperationInfo operationInfo)
		{
			TezosLogger.LogDebug($"Mint completed with operation ID: {operationInfo.Id}");
			
			TezosManager.Instance.EventManager.OperationCompleted -= MintCompleted;

			var owner = _walletAccount.GetWalletAddress();

			var getOwnerTokensCoroutine = _tezosAPI.GetTokensForOwner(GetTokensCallback, owner, true, 10_000,
				new TokensForOwnerOrder.Default(0));

			CoroutineRunner.Instance.StartCoroutine(getOwnerTokensCoroutine);
		}

		private void GetTokensCallback(HttpResult<IEnumerable<TokenBalance>> httpResult)
		{
			if (httpResult.Success)
			{
				_onMintCompleted.Invoke(httpResult.Data.Last());
			}
			else
			{
				TezosLogger.LogError(httpResult.ErrorMessage);
			}
		}

		private void TransferCompleted(OperationInfo operationInfo)
		{
			var transactionHash = operationInfo.Hash;
			_onTransferCompleted.Invoke(transactionHash);
		}

		private void DeployCompleted(OperationInfo operationInfo)
		{
			TezosLogger.LogDebug($"Deploy completed with operation ID: {operationInfo.Id}");
			
			TezosManager.Instance.EventManager.OperationCompleted -= DeployCompleted;

			var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;
			var creator = _walletAccount.GetWalletAddress();

			CoroutineRunner.Instance.StartCoroutine(_tezosAPI.GetOriginatedContractsForOwner(OnGetContracts, creator,
				codeHash, 1000, new OriginatedContractsForOwnerOrder.Default(0)));

			return;

			void OnGetContracts(HttpResult<IEnumerable<TokenContract>> result)
			{
				TezosLogger.LogDebug("Got contracts for owner");

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
					TezosLogger.LogError(result.ErrorMessage);
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