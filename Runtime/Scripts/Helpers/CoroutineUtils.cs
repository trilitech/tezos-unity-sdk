using System;
using UnityEngine;
using System.Collections;

public static class CoroutineUtils
{
    public static Coroutine TryWith(
        MonoBehaviour monoBehaviour,
        IEnumerator coroutine,
        Action<Exception> exceptionHandler = null)
    {
        return monoBehaviour.StartCoroutine(Try(coroutine, exceptionHandler));
    }

    public static IEnumerator Try(
        IEnumerator coroutine,
        Action<Exception> exceptionHandler = null)
    {
        while (true)
        {
            object current;
            try
            {
                if (coroutine.MoveNext() == false)
                {
                    break;
                }
                current = coroutine.Current;
            }
            catch (Exception ex)
            {
                if (exceptionHandler == null)
                {
                    Debug.LogError(ex);
                }
                else
                {
                    exceptionHandler?.Invoke(ex);
                }
                yield break;
            }
            yield return current;
        }
        exceptionHandler?.Invoke(null);
    }
}