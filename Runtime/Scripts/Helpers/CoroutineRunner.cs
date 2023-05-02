using System;
using System.Collections;
using UnityEngine;

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
