using System.Collections.Generic;
using System.Linq;
using Beacon.Sdk.Beacon.Operation;
using Netezos.Contracts;
using Netezos.Encoding;
using Newtonsoft.Json.Linq;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.Token;
using Tezos.WalletProvider;
using UnityEngine;

namespace Tezos.API
{
	public static partial class TezosAPI
	{
		public static async UniTask<TokenBalance> Mint(TokenMetadata tokenMetadata, string destination, int amount, string address)
		{
			TezosLogger.LogDebug($"Minting {amount} tokens to {destination} with metadata {tokenMetadata}");

			var tokens = (await GetTokensForContract(address, false, 10_000, new TokensForContractOrder.Default(0))).ToList();

			TezosLogger.LogDebug("Got tokens for contract");
			var          tokenId    = tokens.Count;
			const string entrypoint = "mint";

			var mintParameters = GetContractScript().BuildParameter(entrypoint, new { address = destination, amount = amount.ToString(), metadata = tokenMetadata.GetMetadataDict(), token_id = tokenId.ToString() }).ToJson();

			var walletOperationRequest = new WalletOperationRequest { Destination = address, EntryPoint = entrypoint, Arg = mintParameters };
			var result                 = await RequestOperation(walletOperationRequest);

			TezosLogger.LogDebug($"Mint completed with operation ID: {result.Id}");

			var owner = GetWalletConnectionData().WalletAddress;

			var getOwnerTokensCoroutine = await GetTokensForOwner(owner, true, 10_000, new TokensForOwnerOrder.Default(0));

			return getOwnerTokensCoroutine.Last();
		}

		public static async UniTask<string> Transfer(string destination, int tokenId, int amount)
		{
			var          activeAddress = GetWalletConnectionData().WalletAddress;
			const string entryPoint    = "transfer";

			var param = GetContractScript().BuildParameter(entryPoint, new List<object> { new { from_ = activeAddress, txs = new List<object> { new { to_ = destination, token_id = tokenId, amount } } } }).ToJson();

			var walletOperationRequest = new WalletOperationRequest { Destination = activeAddress, EntryPoint = entryPoint, Arg = param };

			var result = await RequestOperation(walletOperationRequest);

			return result.TransactionHash;
		}
		
		public static async UniTask<string> Deploy()
		{
			var deployTcs = new UniTaskCompletionSource<string>();
			TezosLogger.LogDebug("Deploying contract...");

			var stringScript    = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
			var address         = GetWalletConnectionData().WalletAddress;
			var scriptWithAdmin = stringScript.Replace("CONTRACT_ADMIN", address);

			var walletOriginateContractRequest = new WalletOriginateContractRequest { Script = scriptWithAdmin };
			OperationResulted += OnOperationResulted;
			await RequestContractOrigination(walletOriginateContractRequest);

			async void OnOperationResulted(OperationResponse operationResponse)
			{
				OperationResulted -= OnOperationResulted;
				TezosLogger.LogDebug($"Deploy completed with operation ID: {operationResponse.Id}");

				var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash").text;
				var creator  = GetWalletConnectionData().WalletAddress;
				
				var result = await GetOriginatedContractsForOwner(creator, codeHash, 1000, new OriginatedContractsForOwnerOrder.Default(0));

				var tokenContracts = result.ToList();

				if (!tokenContracts.Any())
				{
					return;
				}

				var lastUsedContract = tokenContracts.Last();
				PlayerPrefs.SetString("CurrentContract:" + creator, lastUsedContract);
				deployTcs.TrySetResult(lastUsedContract);
			}

			return await deployTcs.Task;
		}

		private static ContractScript GetContractScript()
		{
			var script = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
			var code   = JObject.Parse(script).SelectToken("code");

			return new ContractScript(Micheline.FromJson(code.ToString()));
		}
	}
}