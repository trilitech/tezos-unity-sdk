using System;
using System.Collections.Generic;

namespace TezosSDK.MessageSystem
{
	public class MessageSystem: IMessageSystem
	{
		private Dictionary<Type, object> _listeners = new();

		public void AddListener<T>(Action<T> callback)
		{
			var messageType = typeof(T);
			if (_listeners.TryGetValue(messageType, out var actions))
			{
				var actionList = actions as List<Action<T>>;
				actionList.Add(callback);
			}
			else
			{
				var actionList = new List<Action<T>> { callback };
				_listeners.Add(messageType, actionList);
			}
		}

		public void RemoveListener<T>(Action<T> callback)
		{
			var messageType = typeof(T);
			if (_listeners.TryGetValue(messageType, out var actions))
			{
				var actionList = actions as List<Action<T>>;
				actionList.Remove(callback);
			}
		}

		public void InvokeMessage<T>(T commandMessage)
		{
			var messageType = typeof(T);
			if (_listeners.TryGetValue(messageType, out var actions))
			{
				var actionList = actions as List<Action<T>>;
				var copyList   = new List<Action<T>>(actionList); // preventing enumerating on original list because
				// listeners can un/subscribe and modify the original
				foreach (var action in copyList)
				{
					action?.Invoke(commandMessage);
				}
			}
		}
	}
}
