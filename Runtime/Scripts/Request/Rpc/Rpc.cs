using Tezos.Cysharp.Threading.Tasks;

namespace Tezos.Request
{
	public class Rpc : TezosClient
	{
		public Rpc(int timeOut) : base(timeOut)
		{
		}

		public UniTask<T> GetContractCode<T>(string contract) => GetRequest<T>($"chains/main/blocks/head/context/contracts/{contract}/script/");
	}
}