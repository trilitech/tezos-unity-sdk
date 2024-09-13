using System;
using System.Collections;
using System.Text.Json;
using Netezos.Rpc.Queries.Post;
using TezosSDK.Helpers.HttpClients;

namespace TezosSDK.API
{

	public class Rpc : TezosHttpClient
	{
		private const string CHAIN_ID = "NetXdQprcVkpaWU";

		public Rpc(string rpc, int timeOut) : base(rpc, timeOut)
		{
		}

		public IEnumerator GetTzBalance(string address, Action<HttpResult<long>> callback)
		{
			yield return GetJsonCoroutine<long>($"chains/main/blocks/head/context/contracts/{address}/balance/", result =>
			{
				if (result.Success)
				{
					callback?.Invoke(new HttpResult<long>(result.Data));
				}
				else
				{
					callback?.Invoke(new HttpResult<long>(result.ErrorMessage));
				}
			});
		}

		public IEnumerator GetContractCode<T>(string contract)
		{
			return GetJsonCoroutine<T>($"chains/main/blocks/head/context/contracts/{contract}/script/");
		}

		public IEnumerator RunView<T>(
			string contract,
			string view,
			string input,
			Action<HttpResult<T>> callback,
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

			return PostJsonCoroutine("chains/main/blocks/head/helpers/scripts/run_script_view/", data, callback);
		}
	}

}