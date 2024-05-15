using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.Filters;
using TezosSDK.Tezos.Models.Tokens;

namespace TezosSDK.Tezos.Interfaces.API
{

	public interface ITezosAPI
	{
		/// <summary>
		///     An IEnumerator for reading the account's balance
		///     Can be called in a StartCoroutine()
		/// </summary>
		/// <param name="callback">callback action that runs with the float balance is fetched</param>
		/// <param name="address">tz address</param>
		/// <returns></returns>
		IEnumerator GetTezosBalance(Action<HttpResult<ulong>> callback, string address);

		/// <summary>
		///     An IEnumerator for reading data from a contract view
		///     Can be called in a StartCoroutine()
		/// </summary>
		/// <param name="contractAddress">destination address of the smart contract</param>
		/// <param name="entrypoint">entry point used in the smart contract</param>
		/// <param name="input">parameters called on the entry point</param>
		/// <param name="callback">callback action that runs with the json data is fetched</param>
		/// <returns></returns>
		public IEnumerator ReadView(
			string contractAddress,
			string entrypoint,
			string input,
			Action<HttpResult<JsonElement>> callback);

		// Gets all tokens currently owned by a given address.
		public IEnumerator GetTokensForOwner(
			Action<HttpResult<IEnumerable<TokenBalance>>> callback,
			string owner,
			bool withMetadata,
			long maxItems,
			TokensForOwnerOrder orderBy);

		// Get the owner(s) for a token.
		public IEnumerator GetOwnersForToken(
			Action<HttpResult<IEnumerable<TokenBalance>>> callback,
			string contractAddress,
			uint tokenId,
			long maxItems,
			OwnersForTokenOrder orderBy);

		// Gets all owners for a given token contract.
		public IEnumerator GetOwnersForContract(
			Action<HttpResult<IEnumerable<TokenBalance>>> callback,
			string contractAddress,
			long maxItems,
			OwnersForContractOrder orderBy);

		// Checks whether a wallet holds a token in a given contract.
		public IEnumerator IsHolderOfContract(Action<HttpResult<bool>> callback, string wallet, string contractAddress);

		// Checks whether a wallet holds a particular token.
		public IEnumerator IsHolderOfToken(
			Action<HttpResult<bool>> callback,
			string wallet,
			string contractAddress,
			uint tokenId);

		// Gets the metadata associated with a given token.
		public IEnumerator GetTokenMetadata(
			Action<HttpResult<JsonElement>> callback,
			string contractAddress,
			uint tokenId);

		// Queries token high-level collection/contract level information.
		public IEnumerator GetContractMetadata(Action<HttpResult<JsonElement>> callback, string contractAddress);

		// Gets all tokens for a given token contract.
		public IEnumerator GetTokensForContract(
			Action<HttpResult<IEnumerable<Token>>> callback,
			string contractAddress,
			bool withMetadata,
			long maxItems,
			TokensForContractOrder orderBy);

		// Returns operation status: true if applied, false if failed, null (or HTTP 204) if doesn't exist.
		public IEnumerator GetOperationStatus(Action<HttpResult<bool>> callback, string operationHash);

		// Returns a level of the block closest to the current timestamp.
		public IEnumerator GetLatestBlockLevel(Action<HttpResult<int>> callback);

		// Get account's counter.
		public IEnumerator GetAccountCounter(Action<HttpResult<int>> callback, string address);

		// Get list of originated contracts by creator
		public IEnumerator GetOriginatedContractsForOwner(
			Action<HttpResult<IEnumerable<TokenContract>>> callback,
			string creator,
			string codeHash,
			long maxItems,
			OriginatedContractsForOwnerOrder orderBy);
	}

}