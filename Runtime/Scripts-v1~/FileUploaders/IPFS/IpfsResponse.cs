using System;
using System.Text.Json.Serialization;

namespace TezosSDK.FileUploaders.IPFS
{

	public class IpfsResponse
	{
		public string IpfsHash { get; set; }

		public int PinSize { get; set; }

		public DateTime Timestamp { get; set; }

		[JsonPropertyName("isDuplicate")]
		public bool IsDuplicate { get; set; }
	}

}