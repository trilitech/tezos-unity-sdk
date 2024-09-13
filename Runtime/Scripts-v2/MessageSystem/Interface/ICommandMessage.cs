namespace TezosSDK.MessageSystem
{
	public interface ICommandMessage<T>
	{
		T GetData();
	}
}
