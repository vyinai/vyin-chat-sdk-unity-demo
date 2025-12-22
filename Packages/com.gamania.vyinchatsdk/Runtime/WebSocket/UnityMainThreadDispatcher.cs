// -----------------------------------------------------------------------------
//
// Unity Main Thread Dispatcher
// Allows executing actions on Unity's main thread from background threads
//
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace VyinChatSdk.WebSocket
{
    /// <summary>
    /// Singleton class to dispatch actions to Unity's main thread
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher instance;
        private static readonly Queue<Action> executionQueue = new Queue<Action>();
        private static readonly object queueLock = new object();

        /// <summary>
        /// Get or create the singleton instance
        /// </summary>
        public static UnityMainThreadDispatcher Instance()
        {
            if (instance == null)
            {
                // Find existing instance
                instance = FindObjectOfType<UnityMainThreadDispatcher>();

                if (instance == null)
                {
                    // Create new GameObject with dispatcher
                    GameObject go = new GameObject("UnityMainThreadDispatcher");
                    instance = go.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(go);
                }
            }

            return instance;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Enqueue an action to be executed on the main thread
        /// </summary>
        public void Enqueue(Action action)
        {
            if (action == null)
            {
                return;
            }

            lock (queueLock)
            {
                executionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            // Execute all queued actions on main thread
            lock (queueLock)
            {
                while (executionQueue.Count > 0)
                {
                    Action action = executionQueue.Dequeue();
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[UnityMainThreadDispatcher] Error executing action: {ex.Message}");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
