namespace TezosSDK.MessageSystem
{
	public class Context: IContext
	{
		public IMessageSystem MessageSystem { get; }

		public Context() => MessageSystem = new MessageSystem();
	}
}
