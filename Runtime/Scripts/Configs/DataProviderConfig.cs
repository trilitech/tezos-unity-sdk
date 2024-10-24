using Beacon.Sdk.Beacon.Permission;
using UnityEngine;

namespace Tezos.Configs
{
	[CreateAssetMenu(fileName = "DataProviderConfigSO", menuName = "Tezos/Data Provider Configuration", order = 2)]
	public class DataProviderConfig: ScriptableObject
	{
		[Tooltip("Select the network to use for querying data.")]
		[SerializeField] public NetworkType Network = NetworkType.ghostnet;

		// The URL format string for the base API endpoint. Use {network} as a placeholder for the network type.
		// Example format: "https://api.{network}.tzkt.io/v1/"
		// If NetworkType is set to 'ghostnet', the resulting URL will be: "https://api.ghostnet.tzkt.io/v1/"
		[Tooltip("The URL format for the base API endpoint. Use {network} as a placeholder for the network type.")]
		[SerializeField] private string baseUrlFormat = "https://api.{network}.tzkt.io/v1/";

		[Tooltip("URL to the documentation of the data provider. (Optional)")]
		[SerializeField] private string documentationUrl = "https://api.tzkt.io/";

		public string BaseUrl => baseUrlFormat.Replace("{network}", Network.ToString());
		public string DocumentationUrl => documentationUrl;
	}
}
