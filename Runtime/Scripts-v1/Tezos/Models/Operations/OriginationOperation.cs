using System.Text.Json.Serialization;

namespace TezosSDK.Tezos.Models.Operations
{

	public class OriginationOperation : Operation
	{
		/// <summary>
		///     Originated contract.
		/// </summary>
		[JsonPropertyName("originatedContract")]
		public DeployedContract Contract { get; set; }
	}

}