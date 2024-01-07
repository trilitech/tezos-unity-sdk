using System.Text.Json.Serialization;

namespace TezosSDK.Tezos.API.Models.Operations
{

	public class OriginationOperation : Operation
	{
		/// <summary>
		///     Originated contract.
		/// </summary>
		[JsonPropertyName("originatedContract")]
		public DeployedContract Contract { get; set; }
	}

	public class DeployedContract
	{
		/// <summary>
		///     Address of originated contract.
		/// </summary>
		public string Address { get; set; }
	}

}