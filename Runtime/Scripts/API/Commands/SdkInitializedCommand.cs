using Tezos.MessageSystem;

namespace Tezos.API
{
	public class SdkInitializedCommand : ICommandMessage<bool>
	{
		public bool GetData() => true;
	}
}
