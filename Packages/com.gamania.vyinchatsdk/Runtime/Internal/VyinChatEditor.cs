#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace VyinChatSdk.Internal
{
    internal class VyinChatEditor : IVyinChat
    {
        private static VcHttpClient httpClient;
        private static string currentAppId;
        private static string currentDomain = "gamania.chat";
        private static string currentUserId;
        private static MonoBehaviour coroutineRunner;

        public VyinChatEditor()
        {
            Debug.Log("[VyinChatEditor] Running in Editor mode with HTTP client implementation");
        }

        public void Init(VcInitParams initParams)
        {
            currentAppId = initParams.AppId;
            Debug.Log($"[VyinChatEditor] Init with AppId={currentAppId}");

            // Try to get custom configuration if set
            if (ChatSDKWrapper.HasCustomConfiguration())
            {
                currentDomain = ChatSDKWrapper.GetCustomDomain() ?? currentDomain;
                Debug.Log($"[VyinChatEditor] Using custom domain: {currentDomain}");
            }
        }

        public void Connect(string userId, string authToken, VcUserHandler callback)
        {
            currentUserId = userId;
            Debug.Log($"[VyinChatEditor] Connecting userId={userId}");

            // Initialize HTTP client
            if (string.IsNullOrEmpty(currentAppId))
            {
                Debug.LogError("[VyinChatEditor] AppId is not set. Call Init() first.");
                callback?.Invoke(null, "AppId is not set. Call Init() first.");
                return;
            }

            httpClient = new VcHttpClient(currentAppId, currentDomain, userId);

            // Start WebSocket login to get session key
            var coroutine = ConnectWithWebSocket(userId, callback);
            GetCoroutineRunner().StartCoroutine(coroutine);
        }

        /// <summary>
        /// Connect via WebSocket to obtain session key
        /// </summary>
        private static IEnumerator ConnectWithWebSocket(string userId, VcUserHandler callback)
        {
            Debug.Log("[VyinChatEditor] Starting WebSocket login to get session key...");

            string loginError = null;

            // Connect and login via WebSocket
            yield return httpClient.ConnectAndLogin((sessionKey, error) =>
            {
                loginError = error;
            });

            if (!string.IsNullOrEmpty(loginError))
            {
                Debug.LogError($"[VyinChatEditor] WebSocket login failed: {loginError}");
                callback?.Invoke(null, loginError);
                yield break;
            }

            var user = new VcUser
            {
                UserId = userId,
                Nickname = userId
            };
            Debug.Log($"[VyinChatEditor] Connected as userId={user.UserId}");
            callback?.Invoke(user, null);
        }

        /// <summary>
        /// Create a group channel using HTTP REST API (Editor mode)
        /// </summary>
        public static IEnumerator CreateGroupChannel(
            VcGroupChannelCreateParams channelParams,
            VcGroupChannelCallbackHandler callback)
        {
            if (httpClient == null)
            {
                Debug.LogError("[VyinChatEditor] HTTP client not initialized. Call Connect() first.");
                callback?.Invoke(null, "Not connected. Call Connect() first.");
                yield break;
            }

            Debug.Log($"[VyinChatEditor] Creating channel: {channelParams.Name}");

            bool completed = false;
            VcGroupChannel resultChannel = null;
            string resultError = null;

            // Start HTTP request
            yield return httpClient.CreateGroupChannel(channelParams, (channel, error) =>
            {
                completed = true;
                resultChannel = channel;
                resultError = error;
            });

            // Wait for completion (in case the callback is delayed)
            float timeout = 10f;
            while (!completed && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            if (!completed)
            {
                Debug.LogError("[VyinChatEditor] Request timed out");
                callback?.Invoke(null, "Request timed out");
            }
            else
            {
                callback?.Invoke(resultChannel, resultError);
            }
        }

        /// <summary>
        /// Send a message using HTTP REST API (Editor mode)
        /// </summary>
        public static IEnumerator SendMessage(
            string channelUrl,
            string message,
            VcUserMessageHandler callback)
        {
            if (httpClient == null)
            {
                Debug.LogError("[VyinChatEditor] HTTP client not initialized. Call Connect() first.");
                callback?.Invoke(null, "Not connected. Call Connect() first.");
                yield break;
            }

            Debug.Log($"[VyinChatEditor] Sending message to channel: {channelUrl}");

            bool completed = false;
            VcBaseMessage resultMessage = null;
            string resultError = null;

            // Start HTTP request
            yield return httpClient.SendMessage(channelUrl, message, (msg, error) =>
            {
                completed = true;
                resultMessage = msg;
                resultError = error;
            });

            // Wait for completion
            float timeout = 10f;
            while (!completed && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            if (!completed)
            {
                Debug.LogError("[VyinChatEditor] Request timed out");
                callback?.Invoke(null, "Request timed out");
            }
            else
            {
                callback?.Invoke(resultMessage, resultError);
            }
        }

        /// <summary>
        /// Get or create a MonoBehaviour for running coroutines
        /// </summary>
        public static MonoBehaviour GetCoroutineRunner()
        {
            if (coroutineRunner == null)
            {
                var go = new GameObject("[VyinChat CoroutineRunner]");
                go.hideFlags = HideFlags.HideAndDontSave;
                coroutineRunner = go.AddComponent<CoroutineRunner>();
            }
            return coroutineRunner;
        }

        /// <summary>
        /// Simple MonoBehaviour for running coroutines in Editor mode
        /// </summary>
        private class CoroutineRunner : MonoBehaviour { }
    }
}
#endif