using System.Collections;
using System.Text.Json;
using Netezos.Rpc.Queries.Post;
using TezosSDK.Helpers.HttpClients;

namespace TezosSDK.Tezos.API
{

	public class Rpc : TezosHttpClient
	{
		private const string CHAIN_ID = "NetXdQprcVkpaWU";

		public Rpc(DataProviderConfigSO dataProviderConfig) : base(dataProviderConfig)
		{
		}

		public Rpc(TezosConfigSO rpcConfig) : base(rpcConfig)
		{
		}

		public IEnumerator GetTzBalance<T>(string address)
		{
			return GetJson<T>($"chains/main/blocks/head/context/contracts/{address}/balance/");
		}

		public IEnumerator GetContractCode<T>(string contract)
		{
			return GetJson<T>($"chains/main/blocks/head/context/contracts/{contract}/script/");
		}

		public IEnumerator RunView<T>(
			string contract,
			string view,
			string input,
			string chainId = CHAIN_ID,
			string source = null,
			string payer = null,
			long? gas = null,
			NormalizedQuery.UnparsingMode mode = NormalizedQuery.UnparsingMode.Readable,
			int? now = null,
			int? level = null)
		{
			var data = new
			{
				contract,
				view,
				input = JsonDocument.Parse(input),
				chain_id = chainId,
				unlimited_gas = gas == null,
				unparsing_mode = mode.ToString(),
				source,
				payer,
				gas = gas?.ToString(),
				now = now?.ToString(),
				level = level?.ToString()
			};

			return PostJson<T>("chains/main/blocks/head/helpers/scripts/run_script_view/", data);
		}
	}

}