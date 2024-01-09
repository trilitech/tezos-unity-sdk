using System;
using TezosSDK.Tezos.API.Models.Tokens;

namespace TezosSDK.Tezos
{

	public interface IFa2
	{
		string Address { get; set; }
		int TokensCount { get; set; }
		DateTime LastActivityTime { get; set; }

		/// <summary>
		///     Mint new token on current contract.
		/// </summary>
		/// <param name="completedCallback">
		///     Executes after token minted with minted <see cref="TokenBalance" />.
		/// </param>
		/// <param name="tokenMetadata"><see cref="TokenMetadata" />.</param>
		/// <param name="destination">Address on which token will be minted.</param>
		/// <param name="amount">Amount of minted tokens.</param>
		void Mint(Action<TokenBalance> completedCallback, TokenMetadata tokenMetadata, string destination, int amount);

		/// <summary>
		///     Transfer token from current address to destination.
		/// </summary>
		/// <param name="completedCallback">Executes after token transferred with transaction hash.</param>
		/// <param name="destination">Receiver of a token.</param>
		/// <param name="tokenId">ID of token.</param>
		/// <param name="amount">Amount of transferred tokens.</param>
		void Transfer(Action<string> completedCallback, string destination, int tokenId, int amount);

		/// <summary>
		///     Deploy new instance of FA2 contract.
		/// </summary>
		/// <param name="completedCallback">Executes after contract deployed with contract address.</param>
		void Deploy(Action<string> completedCallback);
	}

}