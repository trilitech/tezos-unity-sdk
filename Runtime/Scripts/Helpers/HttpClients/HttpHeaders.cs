using System.Collections.Generic;

namespace TezosSDK.Helpers.HttpClients
{

	internal static class HttpHeaders
	{
		public static KeyValuePair<string, string> ContentType
		{
			get => new("Content-Type", "application/json");
		}

		public static KeyValuePair<string, string> Accept
		{
			get => new("Accept", "application/json");
		}

		public static KeyValuePair<string, string> UserAgent
		{
			get => new("User-Agent", "tezos-unity-sdk");
		}
	}

}