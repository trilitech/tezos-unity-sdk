using UnityEngine;
using UnityEngine.Serialization;

namespace Tezos.Configs
{
	public enum NetworkType
	{
		mainnet,
		testnet,
	}
	
	[CreateAssetMenu(fileName = "DataProviderConfigSO", menuName = "Tezos/Data Provider Configuration", order = 2)]
	public class DataProviderConfig: ScriptableObject
	{
		[Tooltip("Select the network to use for querying data.")]
		[SerializeField] public NetworkType Network = NetworkType.testnet;

		// The URL format string for the base API endpoint. Use {network} as a placeholder for the network type.
		// Example format: "https://api.{network}.tzkt.io/v1/"
		// If NetworkType is set to 'ghostnet', the resulting URL will be: "https://api.ghostnet.tzkt.io/v1/"
		[FormerlySerializedAs("baseUrlFormatMainnet")]
		[Tooltip("The URL format for the base API endpoint. Use {network} as a placeholder for the network type.")]
		[SerializeField] private string urlMainnet = "https://rpc.tzkt.io/mainnet";
		[SerializeField] private string urlTestnet = "https://rpc.shadownet.teztnets.com";

		[Tooltip("URL to the documentation of the data provider. (Optional)")]
		[SerializeField] private string documentationUrl = "https://api.tzkt.io/";

		public string Rpc => Network == NetworkType.mainnet ? urlMainnet : urlTestnet;
		public string DocumentationUrl => documentationUrl;
	}
}
