/*
Copyright 2015 Pim de Witte All Rights Reserved.
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Patterns;
using UnityEngine;

namespace TezosSDK.Helpers
{

	/// Author: Pim de Witte (pimdewitte.com) and contributors, https://github.com/PimDeWitte/UnityMainThreadDispatcher
	/// <summary>
	///     A thread-safe class which holds a queue with actions to execute on the next Update() method. It can be used to make
	///     calls to the main thread for
	///     things such as UI Manipulation in Unity. It was developed for use in combination with the Firebase Unity plugin,
	///     which uses separate threads for event handling
	/// </summary>
	public class UnityMainThreadDispatcher : SingletonMonoBehaviour<UnityMainThreadDispatcher>
	{
		private static readonly Queue<Action> ExecutionQueue = new();

		public void Update()
		{
			lock (ExecutionQueue)
			{
				while (ExecutionQueue.Count > 0)
				{
					ExecutionQueue.Dequeue().Invoke();
				}
			}
		}

		/// <summary>
		///     Locks the queue and adds the Action to the queue
		/// </summary>
		/// <param name="action">Function that will be executed from the main thread.</param>
		public static void Enqueue(Action action)
		{
			Instance.Enqueue(ActionWrapper(action));
		}

		/// <summary>
		///     Locks the queue and adds the Action to the queue
		/// </summary>
		/// <param name="action">Function that will be executed from the main thread.</param>
		/// <param name="parameter">Function parameter.</param>
		public static void Enqueue<T>(Action<T> action, T parameter)
		{
			Instance.Enqueue(ActionWrapper(action, parameter));
		}

		/// <summary>
		///     Locks the queue and adds the IEnumerator to the queue
		/// </summary>
		/// <param name="action">IEnumerator function that will be executed from the main thread.</param>
		public void Enqueue(IEnumerator action)
		{
			lock (ExecutionQueue)
			{
				var coroutine = new CoroutineWrapper<object>(action, null,
					exception => Debug.LogError($"Exception on MainThread Queue: {exception.Message}"));

				ExecutionQueue.Enqueue(() => { StartCoroutine(coroutine); });
			}
		}

		/// <summary>
		///     Locks the queue and adds the Action to the queue, returning a Task which is completed when the action completes
		/// </summary>
		/// <param name="action">Function that will be executed from the main thread.</param>
		/// <returns>A Task that can be awaited until the action completes</returns>
		public Task EnqueueAsync(Action action)
		{
			var tcs = new TaskCompletionSource<bool>();

			Enqueue(ActionWrapper(WrappedAction));
			return tcs.Task;

			void WrappedAction()
			{
				try
				{
					action();
					tcs.TrySetResult(true);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			}
		}

		private static IEnumerator ActionWrapper(Action a)
		{
			a();
			yield return null;
		}

		private static IEnumerator ActionWrapper<T>(Action<T> function, T parameter)
		{
			yield return null;
			function.Invoke(parameter);
		}
	}

}