using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using TezosSDK.Tezos.API;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;

namespace TezosSDK.Tezos
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
		public IEnumerator GetTezosBalance(Action<ulong> callback, string address);

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
			Action<JsonElement> callback);

		// Gets all tokens currently owned by a given address.
		public IEnumerator GetTokensForOwner(
			Action<IEnumerable<TokenBalance>> callback,
			string owner,
			bool withMetadata,
			long maxItems,
			TokensForOwnerOrder orderBy);

		// Get the owner(s) for a token.
		public IEnumerator GetOwnersForToken(
			Action<IEnumerable<TokenBalance>> callback,
			string contractAddress,
			uint tokenId,
			long maxItems,
			OwnersForTokenOrder orderBy);

		// Gets all owners for a given token contract.
		public IEnumerator GetOwnersForContract(
			Action<IEnumerable<TokenBalance>> callback,
			string contractAddress,
			long maxItems,
			OwnersForContractOrder orderBy);

		// Checks whether a wallet holds a token in a given contract.
		public IEnumerator IsHolderOfContract(Action<bool> callback, string wallet, string contractAddress);

		// Checks whether a wallet holds a particular token.
		public IEnumerator IsHolderOfToken(Action<bool> callback, string wallet, string contractAddress, uint tokenId);

		// Gets the metadata associated with a given token.
		public IEnumerator GetTokenMetadata(Action<TokenMetadata> callback, string contractAddress, uint tokenId);

		// Queries token high-level collection/contract level information.
		public IEnumerator GetContractMetadata(Action<JsonElement> callback, string contractAddress);

		// Gets all tokens for a given token contract.
		public IEnumerator GetTokensForContract(
			Action<IEnumerable<Token>> callback,
			string contractAddress,
			bool withMetadata,
			long maxItems,
			TokensForContractOrder orderBy);

		// Returns operation status: true if applied, false if failed, null (or HTTP 204) if doesn't exist.
		public IEnumerator GetOperationStatus(Action<bool?> callback, string operationHash);

		// Returns a level of the block closest to the current timestamp.
		public IEnumerator GetLatestBlockLevel(Action<int> callback);

		// Get account's counter.
		public IEnumerator GetAccountCounter(Action<int> callback, string address);

		// Get list of originated contracts by creator
		public IEnumerator GetOriginatedContractsForOwner(
			Action<IEnumerable<TokenContract>> callback,
			string creator,
			string codeHash,
			long maxItems,
			OriginatedContractsForOwnerOrder orderBy);
	}

}