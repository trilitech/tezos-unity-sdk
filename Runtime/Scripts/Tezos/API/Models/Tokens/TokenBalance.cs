using System;

namespace TezosSDK.Tezos.API.Models.Tokens
{

	public class TokenBalance
	{
		/// <summary>
		///     Internal TzKT id.
		///     **[sortable]**
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		///     Owner account.
		///     Click on the field to expand more details.
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		///     Balance (raw value, not divided by `decimals`).
		///     **[sortable]**
		/// </summary>
		public string Balance { get; set; }

		/// <summary>
		///     Contract, created the token.
		/// </summary>
		public Alias TokenContract { get; set; }

		/// <summary>
		///     Token id, unique within the contract.
		/// </summary>
		public string TokenId { get; set; }

		/// <summary>
		///     Token metadata.
		/// </summary>
		public TokenMetadata TokenMetadata { get; set; }

		/// <summary>
		///     Timestamp of the block where the token balance was last changed.
		/// </summary>
		public DateTime LastTime { get; set; }
	}

}