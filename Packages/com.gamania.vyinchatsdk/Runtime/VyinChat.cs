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
            Debug.Log("[VyinChat] Platform: Editor (simulated)");
            vyinChatImpl = new Internal.VyinChatEditor();
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
                    Debug.LogWarning("[VyinChat] Unsupported platform");
                    break;
            }
#endif
        }

        public static void Init(VcInitParams initParams)
        {
            TryExecute(() => vyinChatImpl.Init(initParams), "Init");
        }

        public static void Connect(string userId, string authToken, VcUserHandler callback)
        {
            TryExecute(() => vyinChatImpl.Connect(userId, authToken, callback), "Connect");
        }

        /// <summary>
        /// Set custom configuration for ChatSDK
        /// Call this BEFORE Init() to override default settings (iOS only for now)
        /// </summary>
        /// <param name="appId">Application ID (optional, pass null to keep current)</param>
        /// <param name="domain">Environment domain (e.g., "dev.gim.beango.com", "stg.gim.beango.com", "gamania.chat")</param>
        public static void SetConfiguration(string appId, string domain)
        {
#if UNITY_EDITOR
            Debug.Log($"[VyinChat] Simulate SetConfiguration in Editor - appId: {appId}, domain: {domain}");
#else
            if (Application.platform == RuntimePlatform.Android)
            {
                // TODO: Android implementation not yet available
                Debug.LogWarning("[VyinChat] SetConfiguration not implemented on Android yet");
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                try
                {
                    Internal.ChatSDKWrapper.SetConfiguration(appId, domain);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error calling iOS SetConfiguration: " + e);
                }
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
    }
}
