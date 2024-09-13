namespace TezosSDK.MessageSystem
{
	public class Context: IContext
	{
		public IMessageSystem MessageSystem { get; } = new MessageSystem();
	}
}
