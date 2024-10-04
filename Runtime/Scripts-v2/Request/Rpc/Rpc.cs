using System.Text.Json;
using Netezos.Rpc.Queries.Post;
using Tezos.Cysharp.Threading.Tasks;

namespace Tezos.Request
{
	public class Rpc : TezosClient
	{
		private const string CHAIN_ID = "NetXdQprcVkpaWU";

		public Rpc(int timeOut) : base(timeOut)
		{
		}

		public UniTask<T> GetContractCode<T>(string contract) => GetRequest<T>($"chains/main/blocks/head/context/contracts/{contract}/script/");

		public UniTask<T> RunView<T>(
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

			return PostRequest<T>("chains/main/blocks/head/helpers/scripts/run_script_view/", data);
		}
	}
}