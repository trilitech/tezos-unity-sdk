using TezosSDK.MessageSystem;

namespace Tezos.Initializer
{
	public class SdkInitializedCommand : ICommandMessage<bool>
	{
		public bool GetData() => true;
	}
}
