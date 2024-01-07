using System;
using System.Collections;
using TezosSDK.DesignPattern.Singleton;
using UnityEngine;

namespace TezosSDK.Helpers
{

	/// <summary>
	///     Helper class that will allow to run a coroutine
	/// </summary>
	public class CoroutineRunner : SingletonMonoBehaviour<CoroutineRunner>
	{
		[Obsolete("StartCoroutineWrapper is obsolete and will be replaced by StartWrappedCoroutine in future releases")]
		public Coroutine StartCoroutineWrapper(IEnumerator coroutine)
		{
			return StartWrappedCoroutine(coroutine);
		}

		public Coroutine StartWrappedCoroutine(IEnumerator coroutine)
		{
			return StartCoroutine(new CoroutineWrapper<object>(coroutine, null,
				exception => Debug.LogError($"Exception on Coroutine: {exception.Message}")));
		}
	}

}