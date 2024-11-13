using Beacon.Sdk.Beacon.Permission;
using TezosSDK.WalletServices.Connectors;
using UnityEngine;

namespace TezosSDK.Tezos.ScriptableObjects
{

	[CreateAssetMenu(fileName = "TezosConfigSO", menuName = "Tezos/Configuration", order = 1)]
	public class TezosConfigSO : ScriptableObject
	{
		[Tooltip("Select the network to use for chain interactions.")]
		[SerializeField] private NetworkType network = NetworkType.ghostnet;

		[Tooltip("Select connector type to use for wallet interactions.")]
		[SerializeField] private ConnectorType connectorType;

		[Tooltip("Web client address for Kukai Connector.")]
		public string KukaiWebClientAddress;

		// The URL format string for the RPC endpoint. Use {network} as a placeholder for the network type.
		// Example format: "https://{network}.tezos.marigold.dev"
		// If NetworkType is set to 'ghostnet', the resulting URL will be: "https://ghostnet.tezos.marigold.dev"
		[Tooltip("The URL format for the RPC endpoint. Use {network} as a placeholder for the network type.")]
		[SerializeField] private string rpcUrlFormat = "https://{network}.tezos.marigold.dev";

		[Tooltip("Timeout for requests to the chain.")]
		[SerializeField] private int requestTimeoutSeconds = 45;

		[Tooltip("Create API key in Pinata service https://app.pinata.cloud/developers/api-keys and paste JWT value " + "here to be able to upload images to IPFS.")]
		[SerializeField] private string pinataApiKey;

		[Tooltip("Data provider to use for querying data.")]
		[SerializeField] private DataProviderConfigSO dataProviderConfig;

		public ConnectorType ConnectorType
		{
			get => connectorType;
		}

		public DataProviderConfigSO DataProvider
		{
			get => dataProviderConfig;
		}

		public NetworkType Network
		{
			get => network;
		}

		public string PinataApiKey
		{
			get => pinataApiKey;
		}

		public int RequestTimeoutSeconds
		{
			get => requestTimeoutSeconds;
		}

		public string Rpc
		{
			get => rpcUrlFormat.Replace("{network}", network.ToString());
		}

		private void OnValidate()
		{
			if (string.IsNullOrEmpty(pinataApiKey))
			{
				Debug.LogWarning("Pinata API key is not set in TezosConfigSO. You will not be able to upload images to IPFS.");
			}

			if (dataProviderConfig == null)
			{
				Debug.LogError("Data provider is not set in TezosConfigSO. You will not be able to query data.");
			}
			else
			{
				if (dataProviderConfig.Network != network)
				{
					Debug.LogError("Data provider network does not match TezosConfigSO network.");
				}
			}
		}
	}

}