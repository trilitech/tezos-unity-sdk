using Beacon.Sdk.Beacon.Permission;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tezos.Configs
{
	[CreateAssetMenu(fileName = "TezosConfigSO", menuName = "Tezos/Configuration/TezosConfigSO", order = 1)]
	public class TezosConfig: ScriptableObject
	{
		[Tooltip("Select the network to use for chain interactions.")]
		[SerializeField] public NetworkType Network = NetworkType.testnet;

		[Tooltip("Web client address for Kukai Connector.")]
		public string KukaiWebClientAddress;

		[Tooltip("Timeout for requests to the chain.")]
		[SerializeField] private int requestTimeoutSeconds = 45;

		[Tooltip("Create API key in Pinata service https://app.pinata.cloud/developers/api-keys and paste JWT value " + "here to be able to upload images to IPFS.")]
		[SerializeField] private string pinataApiToken;

		[Tooltip("Data provider to use for querying data.")]
		[SerializeField] private DataProviderConfig dataProviderConfig;

		public DataProviderConfig DataProvider
		{
			get => dataProviderConfig;
			set => dataProviderConfig = value;
		}

		public string PinataApiToken => pinataApiToken;

		public int RequestTimeoutSeconds => requestTimeoutSeconds;
	}
}
