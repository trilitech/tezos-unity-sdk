using System;
using System.Collections.Generic;
using System.Linq;
using Netezos.Contracts;
using Netezos.Encoding;
using Newtonsoft.Json.Linq;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.Operation;
using Tezos.Token;
using UnityEngine;
using OperationRequest = Tezos.Operation.OperationRequest;
using OperationResponse = Tezos.Operation.OperationResponse;

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

			var walletOperationRequest = new OperationRequest { Destination = address, EntryPoint = entrypoint, Arg = mintParameters };
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

			var walletOperationRequest = new OperationRequest { Destination = activeAddress, EntryPoint = entryPoint, Arg = param };

			var result = await RequestOperation(walletOperationRequest);

			return result.TransactionHash;
		}

		private static ContractScript GetContractScript() // TODO: This needs to be replaced with the actual contract script, not the conract we ship with the SDK. Netezos possibly has a way of achieving this.
		{
			var script = Resources.Load<TextAsset>("Contracts/FA2TokenContract").text;
			
			if (string.IsNullOrEmpty(script))
			{
				throw new InvalidOperationException("Failed to load contract script");
			}
			
			var code   = JObject.Parse(script).SelectToken("code");

			if (code != null)
			{
				return new ContractScript(Micheline.FromJson(code.ToString())!);
			}
			
			throw new InvalidOperationException("Failed to parse contract code");
		}
	}
}