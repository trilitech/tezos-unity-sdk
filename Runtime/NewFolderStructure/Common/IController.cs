using System.Threading.Tasks;
using TezosSDK.MessageSystem;

namespace TezosSDK.Common
{
	public interface IController
	{
		public bool     IsInitialized { get; }
		public IContext Context       { get; }

		public Task Initialize(IContext context);
	}
}
