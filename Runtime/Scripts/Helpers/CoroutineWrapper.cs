using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Wraps a coroutine allowing to extract the result when it's completed
/// </summary>
/// <typeparam name="T">Type of result expected</typeparam>
public class CoroutineWrapper<T> : IEnumerator
{
    /// <summary>
    /// Event raised when the coroutine is complete
    /// </summary>
    public readonly Action<T> Completed;

    private readonly IEnumerator _targetCoroutine;

    /// <summary>
    /// Exception triggered during the execution of the coroutine.
    /// </summary>
    public Exception Exception { get; private set; }

    /// <inheritdoc/>
    public object Current { get; private set; }

    /// <summary>
    /// Result extracted from the coroutine when it's complete
    /// </summary>
    public T Result { get; private set; }

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
            Debug.LogError("Exception " + e.Message);
            Exception = e;
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