using Beacon.Sdk.Beacon.Permission;
using UnityEngine;

namespace TezosSDK.Tezos
{

	[CreateAssetMenu(fileName = "DataProviderConfigSO", menuName = "Tezos/Data Provider Configuration", order = 2)]
	public class DataProviderConfigSO : ScriptableObject
	{
		[Tooltip("Select the network to use for querying data.")]
		[SerializeField] private NetworkType network = NetworkType.ghostnet;

		// The URL format string for the base API endpoint. Use {network} as a placeholder for the network type.
		// Example format: "https://api.{network}.tzkt.io/v1/"
		// If NetworkType is set to 'ghostnet', the resulting URL will be: "https://api.ghostnet.tzkt.io/v1/"
		[Tooltip("The URL format for the base API endpoint. Use {network} as a placeholder for the network type.")]
		[SerializeField] private string baseUrlFormat = "https://api.{network}.tzkt.io/v1/";

		[Tooltip("Timeout for requests to the data provider.")]
		[SerializeField] private int requestTimeoutSeconds = 45;

		[Tooltip("URL to the documentation of the data provider. (Optional)")]
		[SerializeField] private string documentationUrl = "https://api.tzkt.io/";

		public string BaseUrl
		{
			get => baseUrlFormat.Replace("{network}", network.ToString());
		}

		public string DocumentationUrl
		{
			get => documentationUrl;
		}

		public NetworkType Network
		{
			get => network;
		}

		public int RequestTimeoutSeconds
		{
			get => requestTimeoutSeconds;
		}
	}

}