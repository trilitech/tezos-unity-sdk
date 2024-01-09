using UnityEngine;

namespace TezosSDK.Patterns
{

	public abstract class SingletonMonoBehaviour : MonoBehaviour
	{
	}

	/// <summary>
	///     Easily allow a Singleton to be added to hierarchy at runtime with full MonoBehavior access and predictable
	///     lifecycle.
	/// </summary>
	public abstract class SingletonMonoBehaviour<T> : SingletonMonoBehaviour where T : MonoBehaviour
	{
		public static OnDestroyingDelegate OnDestroying;
		public static OnInstantiateCompletedDelegate OnInstantiateCompleted;

		/// <summary>
		///     Do not call this from another scope within OnDestroy(). Instead use IsInstantiated()
		/// </summary>
		private static T instance; //Harmless 'suggestion' appears here in some code-editors. Known issue.

		public delegate void OnDestroyingDelegate(T instance);

		public delegate void OnInstantiateCompletedDelegate(T instance);

		public static T Instance
		{
			//NOTE: Its recommended to wrap any calls to this getter with a IsInstanced() to prevent undesired instantiation. Optional.
			get
			{
				if (!IsInstantiated())
				{
					Instantiate();
				}

				return instance;
			}
			set => instance = value;
		}

		protected virtual void Awake()
		{
			Instantiate();
		}

		protected virtual void OnDestroy()
		{
		}

		/// <summary>
		///     Destroys all memory/references associated with the instance
		/// </summary>
		public static void Destroy()
		{
			if (!IsInstantiated())
			{
				return;
			}

			OnDestroying?.Invoke(instance);

			// NOTE: Use 'DestroyImmediate'. At runtime its less important, but occasionally editor classes will call Destroy();
			DestroyImmediate(instance.gameObject);

			instance = null;
		}

		/// <summary>
		///     NOTE: Calling this will NEVER instantiate a new instance. That is useful and safe to call in any destructors /
		///     OnDestroy()
		/// </summary>
		/// <returns><c>true</c> if is instantiated; otherwise, <c>false</c>.</returns>
		public static bool IsInstantiated()
		{
			return instance != null;
		}

		/// <summary>
		///     Instantiate this instance.
		///     1. Attempts to find an existing GameObject that matches (There will be 0 or 1 at any time)
		///     2. Creates GameObject with name of subclass
		///     3. Persists by default (optional)
		///     4. Predictable life-cycle.
		/// </summary>
		private static void Instantiate()
		{
			if (IsInstantiated())
			{
				return;
			}

			var t = FindObjectOfType<T>();
			GameObject go = null;

			if (t != null)
			{
				go = t.gameObject;
			}

			if (go == null)
			{
				go = new GameObject();
				instance = go.AddComponent<T>();
			}
			else
			{
				instance = go.GetComponent<T>();
			}

			go.name = instance.GetType().Name;

			//KLUGE: Must unparent/reparent before DontDestroyOnLoad to avoid error
			var parent = go.transform.parent;
			go.transform.SetParent(null);
			DontDestroyOnLoad(go);
			go.transform.SetParent(parent);

			OnInstantiateCompleted?.Invoke(instance);
		}
	}

}