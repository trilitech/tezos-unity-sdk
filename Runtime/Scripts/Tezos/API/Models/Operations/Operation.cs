namespace TezosSDK.Tezos.API.Models.Operations
{

	public class Operation
	{
		/// <summary>
		///     Internal TzKT id (not the same as `tokenId`).
		///     **[sortable]**
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		///     Operation type
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		///     Operation hash
		/// </summary>
		public string Hash { get; set; }
	}

}