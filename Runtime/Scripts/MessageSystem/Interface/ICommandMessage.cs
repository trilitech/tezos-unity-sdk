namespace Tezos.MessageSystem
{
	public interface ICommandMessage<T>
	{
		T GetData();
	}
}
