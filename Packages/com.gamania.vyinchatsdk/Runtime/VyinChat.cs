// -----------------------------------------------------------------------------
//
// Runtime SDK main entry
//
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Newtonsoft.Json;

namespace VyinChatSdk
{
    public static class VyinChat
    {
        private static readonly Internal.IVyinChat vyinChatImpl;

        static VyinChat()
        {
#if UNITY_EDITOR
            // Unity Editor uses Pure C# implementation to connect to real server
            Debug.Log("[VyinChat] Platform: Editor (Pure C# implementation)");
            vyinChatImpl = new Internal.Platform.VyinChatMain();
#else
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    Debug.Log("[VyinChat] Platform: Android");
                    vyinChatImpl = new Internal.VyinChatAndroid();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    Debug.Log("[VyinChat] Platform: iOS");
                    vyinChatImpl = new Internal.VyinChatIOS();
                    break;
                default:
                    Debug.Log("[VyinChat] Platform: Unsupported, using Pure C# implementation");
                    vyinChatImpl = new Internal.Platform.VyinChatMain();
                    break;
            }
#endif
        }

        private static VcInitParams _initParams;

        /// <summary>
        /// Gets initializing state
        /// </summary>
        /// <returns>If true, VyinChat instance is initialized</returns>
        public static bool IsInitialized => _initParams != null;

        /// <summary>
        /// Gets whether local caching is enabled
        /// </summary>
        public static bool UseLocalCaching => _initParams?.IsLocalCachingEnabled ?? false;

        /// <summary>
        /// Gets the Application ID which was used for initialization
        /// </summary>
        /// <returns>The Application ID, or null if not initialized</returns>
        public static string GetApplicationId() => _initParams?.AppId;

        /// <summary>
        /// Gets the log level
        /// </summary>
        /// <returns>The log level</returns>
        public static VcLogLevel GetLogLevel() => _initParams?.LogLevel ?? VcLogLevel.None;

        /// <summary>
        /// Gets the app version
        /// </summary>
        /// <returns>The app version</returns>
        public static string GetAppVersion() => _initParams?.AppVersion;

        /// <summary>
        /// Initializes VyinChat singleton instance with VyinChat Application ID
        /// This method must be run first in order to use VyinChat
        /// </summary>
        /// <param name="initParams">VcInitParams object</param>
        /// <returns>true if the applicationId is set successfully</returns>
        public static bool Init(VcInitParams initParams)
        {
            // Check for null params
            if (initParams == null)
            {
                Debug.LogError("[VyinChat] Init failed: initParams is null");
                return false;
            }

            // Check for empty appId
            if (string.IsNullOrEmpty(initParams.AppId))
            {
                Debug.LogError("[VyinChat] Init failed: AppId is empty");
                return false;
            }

            // Check if already initialized with different appId
            if (_initParams != null && _initParams.AppId != initParams.AppId)
            {
                Debug.LogError($"[VyinChat] Init failed: App ID needs to be the same as the previous one. " +
                    $"Previous: {_initParams.AppId}, New: {initParams.AppId}");
                return false;
            }

            // Set init params
            _initParams = initParams;

            // Call platform-specific implementation
            try
            {
                vyinChatImpl.Init(initParams);
                Debug.Log($"[VyinChat] Initialized successfully with AppId: {initParams.AppId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VyinChat] Init failed with exception: {e.Message}");
                _initParams = null;
                return false;
            }
        }

        public static void Connect(string userId, string authToken, VcUserHandler callback)
        {
            Connect(userId, authToken, null, null, callback);
        }

        /// <summary>
        /// Connect with explicit API and WebSocket hosts
        /// Pure C# implementation: Uses provided hosts for HTTP and WebSocket connections
        /// Legacy implementations: Use SetConfiguration() before calling Connect with null hosts
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="authToken">Optional auth token</param>
        /// <param name="apiHost">API host URL (e.g., "https://api.gamania.chat"), null for default</param>
        /// <param name="wsHost">WebSocket host URL (e.g., "wss://ws.gamania.chat"), null for default</param>
        /// <param name="callback">Callback with user or error</param>
        public static void Connect(string userId, string authToken, string apiHost, string wsHost, VcUserHandler callback)
        {
            TryExecute(() => vyinChatImpl.Connect(userId, authToken, apiHost, wsHost, callback), "Connect");
        }

        /// <summary>
        /// Set custom configuration for ChatSDK
        /// Call this BEFORE Init() to override default settings
        ///
        /// TODO: This method will be removed in the future.
        /// Only iOS platform supports this. Editor and other platforms will show warnings.
        /// </summary>
        /// <param name="appId">Application ID (optional, pass null to keep current)</param>
        /// <param name="domain">Environment domain (e.g., "dev.gim.beango.com", "stg.gim.beango.com", "gamania.chat")</param>
        public static void SetConfiguration(string appId, string domain)
        {
#if UNITY_EDITOR
            // Unity Editor uses Pure C# implementation which doesn't support SetConfiguration yet
            Debug.LogWarning("[VyinChat] Editor mode (Pure C#) does not support SetConfiguration. " +
                "Please use Init() with appropriate parameters or set configuration before static constructor.");
#else
            if (Application.platform == RuntimePlatform.Android)
            {
                // TODO: Android implementation not yet available
                Debug.LogWarning("[VyinChat] SetConfiguration not implemented on Android yet");
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                 Internal.ChatSDKWrapper.SetConfiguration(appId, domain);
            }
            else
            {
                // Other platforms use Pure C# implementation
                Debug.LogWarning("[VyinChat] Pure C# implementation does not support SetConfiguration. " +
                    "Please use Init() with appropriate parameters.");
            }
#endif
        }

        /// <summary>
        /// Reset configuration to default values (PROD environment)
        /// iOS only for now
        /// </summary>
        public static void ResetConfiguration()
        {
#if UNITY_EDITOR
            Debug.Log("[VyinChat] Simulate ResetConfiguration in Editor");
#else
            if (Application.platform == RuntimePlatform.Android)
            {
                // TODO: Android implementation not yet available
                Debug.LogWarning("[VyinChat] ResetConfiguration not implemented on Android yet");
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                try
                {
                    Internal.ChatSDKWrapper.ResetConfiguration();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error calling iOS ResetConfiguration: " + e);
                }
            }
#endif
        }

        // TODO: replace with platform implementation
        public static void InviteUsers(string channelUrl, string[] userIds, Action<string, string> callback)
        {
            TryExecute(() =>
            {
#if UNITY_EDITOR
                SimulateEditorCall(() =>
                {
                    string fakeResult = $"{{\"success\":true,\"channelUrl\":\"{channelUrl}\"}}";
                    callback?.Invoke(fakeResult, null);
                });
#else
                if (Application.platform == RuntimePlatform.Android)
                {
                    // TODO: Android implementation not yet available
                    // Please implement SendMessage in Android UnityBridge
                    Debug.LogWarning("[VyinChat] SendMessage not implemented on Android yet");
                    callback?.Invoke(null, "Not implemented on Android");
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    try
                    {
                        Internal.ChatSDKWrapper.InviteUsers(channelUrl, userIds, callback);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error calling iOS InviteUsers: " + e);
                        callback?.Invoke(null, e.Message);
                    }
                }
#endif
            }, "InviteUsers");
        }

        // TODO: replace with platform implementation
        public static void SendMessage(string channelUrl, string message, Action<string, string> callback)
        {
            TryExecute(() =>
            {
#if UNITY_EDITOR
                SimulateEditorCall(() =>
                {
                    string fakeResult = $"{{\"messageId\":{DateTime.Now.Ticks},\"message\":\"{message}\"}}";
                    callback?.Invoke(fakeResult, null);
                });
#else
                if (Application.platform == RuntimePlatform.Android)
                {
                    try
                    {
                        using var androidBridge =
                            new AndroidJavaClass("com.gamania.gim.unitybridge.UnityBridge");

                        void handler(VcBaseMessage baseMessage, string error)
                        {
                            if (!string.IsNullOrEmpty(error))
                            {
                                callback?.Invoke(null, error);
                                Debug.LogError("[VyinChat] Error calling SendMessage: " + error);
                                return;
                            }
                            string resultJson = JsonConvert.SerializeObject(baseMessage);
                            callback?.Invoke(resultJson, null);
                        }

                        var proxy = new AndroidUserMessageHandlerProxy(handler);
                        androidBridge.CallStatic("sendMessage", channelUrl, message, proxy);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error calling Android SendMessage: " + e);
                        callback?.Invoke(null, e.Message);
                    }
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    try
                    {
                        Internal.ChatSDKWrapper.SendMessage(channelUrl, message, callback);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error calling iOS SendMessage: " + e);
                        callback?.Invoke(null, e.Message);
                    }
                }
#endif
            }, "SendMessage");
        }

        private class AndroidUserMessageHandlerProxy : AndroidJavaProxy
        {
            private readonly VcUserMessageHandler handler;

            public AndroidUserMessageHandlerProxy(VcUserMessageHandler handler)
                : base("com.gamania.gim.sdk.handler.UserMessageHandler")
            {
                this.handler = handler;
            }

            void onResult(AndroidJavaObject userMessage, AndroidJavaObject exception)
            {
                Debug.Log("onResult: userMessage=" + userMessage + ", exception=" + exception);
                var error = exception.GetErrorMessage();
                if (!string.IsNullOrEmpty(error))
                {
                    handler?.Invoke(null, error);
                    Debug.LogError("onResult: error=" + error);
                    return;
                }

                var message = userMessage.ToVcBaseMessage();
                Debug.Log("onResult: message=" + message);
                handler?.Invoke(message, null);
            }
        }

        #region Helper Methods


        public static void TryExecute(Action action, string actionName)
        {
            try
            {
                action.Invoke();
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException (e.g., not initialized)
                // so tests can catch it
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VyinChat] Error in {actionName}: {e.Message}");
            }
        }

#if UNITY_EDITOR
        private static void SimulateEditorCall(Action editorAction)
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    editorAction.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("[VyinChat][Editor Simulate] " + e.Message);
                }
            };
        }
#endif

        #endregion

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        /// <summary>
        /// Reset VyinChat state (for testing only)
        /// WARNING: This is only for testing purposes. Do not use in production code.
        /// </summary>
        public static void ResetForTesting()
        {
            _initParams = null;

            // Reset the implementation instance state
            if (vyinChatImpl is Internal.Platform.VyinChatMain vyinChatMain)
            {
                vyinChatMain.Reset();
            }
        }
#endif
    }
}
