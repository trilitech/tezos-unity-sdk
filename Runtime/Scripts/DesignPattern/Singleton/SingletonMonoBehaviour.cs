using UnityEngine;

namespace TezosSDK.DesignPattern.Singleton
{
    public abstract class SingletonMonoBehaviour : MonoBehaviour { }

    /// <summary>
    /// Easily allow a Singleton to be added to hierarchy at runtime with full MonoBehavior access and predictable lifecycle.
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : SingletonMonoBehaviour
        where T : MonoBehaviour
    {
        /// <summary>
        /// Do not call this from another scope within OnDestroy(). Instead use IsInstantiated()
        /// </summary>
        private static T _instance; //Harmless 'suggestion' appears here in some code-editors. Known issue.
        public static T Instance
        {
            //NOTE: Its recommended to wrap any calls to this getter with a IsInstanced() to prevent undesired instantiation. Optional.
            get
            {
                if (!IsInstantiated())
                {
                    Instantiate();
                }
                return _instance;
            }
            set { _instance = value; }
        }

        /// <summary>
        /// NOTE: Calling this will NEVER instantiate a new instance. That is useful and safe to call in any destructors / OnDestroy()
        /// </summary>
        /// <returns><c>true</c> if is instantiated; otherwise, <c>false</c>.</returns>
        public static bool IsInstantiated()
        {
            return _instance != null;
        }

        public delegate void OnInstantiateCompletedDelegate(T instance);
        public static OnInstantiateCompletedDelegate OnInstantiateCompleted;

        public delegate void OnDestroyingDelegate(T instance);
        public static OnDestroyingDelegate OnDestroying;

        /// <summary>
        /// Instantiate this instance.
        /// 	1. Attempts to find an existing GameObject that matches (There will be 0 or 1 at any time)
        /// 	2. Creates GameObject with name of subclass
        /// 	3. Persists by default (optional)
        /// 	4. Predictable life-cycle.
        /// </summary>
        public static T Instantiate()
        {
            if (IsInstantiated()) return _instance;

            var t = GameObject.FindObjectOfType<T>();
            GameObject go = null;
            if (t != null)
            {
                go = t.gameObject;
            }

            if (go == null)
            {
                go = new GameObject();
                _instance = go.AddComponent<T>();
            }
            else
            {
                _instance = go.GetComponent<T>();
            }

            go.name = _instance.GetType().Name;

            //KLUGE: Must unparent/reparent before DontDestroyOnLoad to avoid error
            var parent = go.transform.parent;
            go.transform.SetParent(null);
            DontDestroyOnLoad(go);
            go.transform.SetParent(parent);

            if (OnInstantiateCompleted != null)
            {
                OnInstantiateCompleted(_instance);
            }
            return _instance;
        }

        protected virtual void Awake()
        {
            Instantiate();
        }

        /// <summary>
        /// Destroys all memory/references associated with the instance
        /// </summary>
        public static void Destroy()
        {
            if (!IsInstantiated()) return;
            
            if (OnDestroying != null)
            {
                OnDestroying(_instance);
            }

            // NOTE: Use 'DestroyImmediate'. At runtime its less important, but occasionally editor classes will call Destroy();
            DestroyImmediate(_instance.gameObject);

            _instance = null;
        }

        protected virtual void OnDestroy() { }
    }
}