using Beacon.Sdk.Beacon.Permission;
using UnityEngine;

namespace TezosSDK.Tezos
{

	[CreateAssetMenu(fileName = "TezosConfigSO", menuName = "Tezos/Configuration", order = 1)]
	public class TezosConfigSO : ScriptableObject
	{
		[SerializeField] private NetworkType network = NetworkType.ghostnet;

		[SerializeField] private int requestTimeoutSeconds = 45;

		[Tooltip("Create API key in Pinata service https://app.pinata.cloud/developers/api-keys and paste JWT value " +
		         "here to be able to upload images to IPFS.")]
		[SerializeField] private string pinataApiKey;

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
			get => $"https://{network.ToString()}.tezos.marigold.dev";
		}
	}

}