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
	}
}