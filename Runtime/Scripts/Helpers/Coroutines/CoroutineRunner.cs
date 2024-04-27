using System.Collections;
using TezosSDK.Patterns;
using UnityEngine;

namespace TezosSDK.Helpers.Coroutines
{

	/// <summary>
	///     Helper class that will allow to run a coroutine
	/// </summary>
	public class CoroutineRunner : SingletonMonoBehaviour<CoroutineRunner>
	{
		public Coroutine StartWrappedCoroutine(IEnumerator coroutine)
		{
			return StartCoroutine(new CoroutineWrapper<object>(coroutine, null,
				exception => Debug.LogError($"Exception on Coroutine: {exception.Message}")));
		}
	}

}