using System;
using System.Collections.Generic;
using UnityEngine;

namespace VyinChatSdk.Internal.Platform
{
    /// <summary>
    /// Dispatcher to execute callbacks on Unity's main thread.
    /// Solves the issue where callbacks from native Android/iOS are executed on background threads,
    /// which causes crashes when updating UI.
    /// </summary>
    internal class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();
        private static readonly object _lock = new object();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var _ = Instance;
        }

        static MainThreadDispatcher()
        {
            Application.quitting += OnApplicationQuit;
        }

        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            var go = new GameObject("VyinChatMainThreadDispatcher");
                            _instance = go.AddComponent<MainThreadDispatcher>();
                            DontDestroyOnLoad(go);
                            Debug.Log("[MainThreadDispatcher] Initialized");
                        }
                    }
                }
                return _instance;
            }
        }

        private static void OnApplicationQuit()
        {
            _instance = null;
        }

        void OnDestroy()
        {
            lock (_lock)
            {
                _executionQueue.Clear();
            }
            if (_instance == this)
            {
                _instance = null;
            }
        }

        void Update()
        {
            lock (_lock)
            {
                while (_executionQueue.Count > 0)
                {
                    var action = _executionQueue.Dequeue();
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[MainThreadDispatcher] Error executing action: {e}");
                    }
                }
            }
        }

        /// <summary>
        /// Enqueue action to be executed on Unity's main thread in the next Update()
        /// </summary>
        public static void Enqueue(Action action)
        {
            if (action == null) return;

            var _ = Instance;

            lock (_lock)
            {
                _executionQueue.Enqueue(action);
            }
        }

#if UNITY_INCLUDE_TESTS
        /// <summary>
        /// Clear all pending actions in the queue (for testing purposes only)
        /// </summary>
        internal static void ClearQueue()
        {
            var _ = Instance;
            lock (_lock)
            {
                _executionQueue.Clear();
            }
        }
#endif
    }
}
