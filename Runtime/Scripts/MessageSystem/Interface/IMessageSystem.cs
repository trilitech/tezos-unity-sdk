using System;

namespace Tezos.MessageSystem
{
	public interface IMessageSystem
	{
		void AddListener<T>(Action<T>    callback);
		void RemoveListener<T>(Action<T> callback);
		void InvokeMessage<T>(T          commandMessage);
	}
}
