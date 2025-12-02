using Beacon.Sdk.Beacon.Permission;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tezos.Configs
{
	public enum NetworkType
	{
		mainnet,
		testnet,
	}

	[CreateAssetMenu(fileName = "TezosConfigSO", menuName = "Tezos/Configuration/TezosConfigSO", order = 1)]
	public class TezosConfig : ScriptableObject
	{
		[Tooltip("Select the network to use for chain interactions.")] [SerializeField]
		public NetworkType Network = NetworkType.testnet;

		// The URL format string for the base API endpoint. Use {network} as a placeholder for the network type.
		// Example format: "https://api.{network}.tzkt.io/v1/"
		// If NetworkType is set to 'ghostnet', the resulting URL will be: "https://api.ghostnet.tzkt.io/v1/"
		[FormerlySerializedAs("baseUrlFormatMainnet")] [Tooltip("The URL format for the base API endpoint. Use {network} as a placeholder for the network type.")] [SerializeField]
		private string urlMainnet = "https://rpc.tzkt.io/mainnet";
		[SerializeField] private string urlTestnet = "https://rpc.shadownet.teztnets.com";

		[Tooltip("URL to the documentation of the data provider. (Optional)")] [SerializeField]
		private string documentationUrl = "https://api.tzkt.io/";

		public string Rpc              => Network == NetworkType.mainnet ? urlMainnet : urlTestnet;
		public string DocumentationUrl => documentationUrl;

		[Tooltip("Web client address for Kukai Connector.")]
		public string KukaiWebClientAddress;

		[Tooltip("Timeout for requests to the chain.")] [SerializeField]
		private int requestTimeoutSeconds = 45;

		[Tooltip("Create API key in Pinata service https://app.pinata.cloud/developers/api-keys and paste JWT value " + "here to be able to upload images to IPFS.")] [SerializeField]
		private string pinataApiToken;

		public string PinataApiToken => pinataApiToken;

		public int RequestTimeoutSeconds => requestTimeoutSeconds;
	}
}