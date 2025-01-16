using Tezos.Cysharp.Threading.Tasks;
using Tezos.MessageSystem;

namespace Tezos.Common
{
	public interface IController
	{
		public bool IsInitialized { get; }

		public UniTask Initialize(IContext context);
	}
}
