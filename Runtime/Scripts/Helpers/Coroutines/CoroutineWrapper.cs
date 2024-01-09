using System;
using System.Collections;
using UnityEngine;

namespace TezosSDK.Helpers.Coroutines
{

	/// <summary>
	///     Wraps a coroutine allowing to extract the result when it's completed
	/// </summary>
	/// <typeparam name="T">Type of result expected</typeparam>
	public class CoroutineWrapper<T> : IEnumerator
	{
		private readonly IEnumerator _targetCoroutine;
		/// <summary>
		///     Event raised when the coroutine is complete
		/// </summary>
		public readonly Action<T> Completed;

		/// <summary>
		///     Event raised when the coroutine throws an exception
		/// </summary>
		public readonly Action<Exception> ErrorHandler;

		/// <summary>
		///     Create an instance of the wrapper
		/// </summary>
		/// <param name="coroutine">Coroutine that will be executed</param>
		/// <param name="callback">Callback that will be called when the coroutine is complete</param>
		/// <param name="errorHandler">Callback that will be called when the coroutine throws an exception</param>
		public CoroutineWrapper(IEnumerator coroutine, Action<T> callback = null, Action<Exception> errorHandler = null)
		{
			_targetCoroutine = coroutine;

			if (callback != null)
			{
				Completed += callback;
			}

			if (errorHandler != null)
			{
				ErrorHandler += errorHandler;
			}
		}

		/// <summary>
		///     Exception triggered during the execution of the coroutine.
		/// </summary>
		public Exception Exception { get; private set; }

		/// <summary>
		///     Result extracted from the coroutine when it's complete
		/// </summary>
		public T Result { get; private set; }

		/// <inheritdoc />
		public object Current { get; private set; }

		/// <inheritdoc />
		public bool MoveNext()
		{
			try
			{
				if (_targetCoroutine.MoveNext())
				{
					Current = _targetCoroutine.Current;
					return true;
				}

				Result = (T)_targetCoroutine.Current;
				Current = _targetCoroutine.Current;
				Completed?.Invoke(Result);
				return false;
			}
			catch (Exception e)
			{
				Exception = e;

				if (ErrorHandler == null)
				{
					Debug.LogError($"Exception: {e.Message}");
				}
				else
				{
					ErrorHandler?.Invoke(e);
				}

				return false;
			}
		}

		/// <inheritdoc />
		public void Reset()
		{
			_targetCoroutine.Reset();
		}
	}

}