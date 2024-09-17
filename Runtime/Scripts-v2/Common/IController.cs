using System.Threading.Tasks;
using Tezos.MessageSystem;

namespace Tezos.Common
{
	public interface IController
	{
		public bool IsInitialized { get; }

		public Task Initialize(IContext context);
	}
}
