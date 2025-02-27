﻿using System.Text.Json.Serialization;

namespace Tezos.Token
{

	public class Alias
	{
		/// <summary>
		///     Account alias name (off-chain data).
		/// </summary>
		[JsonPropertyName("alias")]
		public string Name { get; set; }

		/// <summary>
		///     Account address (public key hash).
		/// </summary>
		public string Address { get; set; }
	}

}