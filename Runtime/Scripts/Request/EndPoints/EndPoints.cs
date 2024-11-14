using System.IO;
using System.Web;
using Tezos.Configs;
using Tezos.MessageSystem;

namespace Tezos.Request
{
	public static class EndPoints
	{
		private static string _baseUrl;

		static EndPoints() => _baseUrl = ConfigGetter.GetOrCreateConfig<DataProviderConfig>().BaseUrl;

		public static string GetBalanceEndPoint(string         walletAddress)         => Path.Combine(_baseUrl, "accounts", walletAddress, "balance");
		public static string GetRunViewEndPoint(string         contract, string name) => Path.Combine(_baseUrl, $"helpers/view/{contract}/{name}");
		public static string GetOperationStatusEndPoint(string operationHash) => Path.Combine(_baseUrl, $"operations/{operationHash}/status");

		public static string GetTokenMetadataEndPoint(string tokenId)
		{
			var url         = Path.Combine(_baseUrl, "tokens");
			var queryParams = HttpUtility.ParseQueryString(string.Empty);
			queryParams["tokenId"] = tokenId;
			queryParams["limit"]   = "1";
			queryParams["select"]  = "metadata";
			return $"{url}?{queryParams}";
		}

		public static string GetTokensEndPoint(string address, int limit = 10)
		{
			var url         = Path.Combine(_baseUrl, "tokens");
			var queryParams = HttpUtility.ParseQueryString(string.Empty);
			queryParams["sender"] = address;
			queryParams["limit"]  = limit.ToString();
			return $"{url}?{queryParams}";
		}
	}
}