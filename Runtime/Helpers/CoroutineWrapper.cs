using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Wraps a coroutine allowing to extract the result when it's completed
/// </summary>
/// <typeparam name="T">Type of result expected</typeparam>
public class CoroutineWrapper<T>: IEnumerator
{
	/// <summary>
	/// Event raised when the coroutine is complete
	/// </summary>
	public Action<T> Completed;

	private IEnumerator _targetCoroutine;

	private Exception _exception;
	/// <summary>
	/// Exception triggered during the execution of the coroutine.
	/// </summary>
	public Exception Exeption => _exception;

	/// <inheritdoc/>
	public object Current => _current;
	public object _current;

	private T _result;
	/// <summary>
	/// Result extracted from the coroutine when it's complete
	/// </summary>
	public T Result => _result;

	/// <summary>
	/// Create an instance of the wrapper
	/// </summary>
	/// <param name="coroutine">Coroutine that will be executed</param>
	/// <param name="callback">Callback that will be called when the coroutine is complete</param>
	public CoroutineWrapper(IEnumerator coroutine, Action<T> callback = null)
	{
		_targetCoroutine = coroutine;
		if (callback != null)
		{
			Completed += callback;
		}
	}

	/// <inheritdoc/>
	public bool MoveNext()
	{
		try
		{
			if (_targetCoroutine.MoveNext())
			{
				_current = _targetCoroutine.Current;

				return true;
			}
			else
			{
				_result = (T)_targetCoroutine.Current;
				_current = _targetCoroutine.Current;
	
				Completed?.Invoke(_result);
				return false;
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Exception " + e.Message);
			_exception = e;
			Completed?.Invoke(default);
			return false;
		}
	}

	/// <inheritdoc/>
	public void Reset()
	{
		_targetCoroutine.Reset();
	}
}

/// <summary>
/// Helper class that will allow to run a coroutine
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
	private static CoroutineRunner _instance;
	public static CoroutineRunner Instance
	{
		get
		{
			if (_instance == null)
				_instance = (new GameObject("CoroutineRunner")).AddComponent<CoroutineRunner>();
			
			return _instance;
		}
	}
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}