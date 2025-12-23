using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VyinChatSdk
{
    public static class VcGroupChannelModule
    {
        public static void CreateGroupChannel(
            VcGroupChannelCreateParams inChannelCreateParams,
            VcGroupChannelCallbackHandler inGroupChannelCallbackHandler)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                Debug.Log($"[VcGroupChannelModule] Creating channel in Editor mode: {inChannelCreateParams.Name}");
                var coroutine = Internal.VyinChatEditor.CreateGroupChannel(inChannelCreateParams, inGroupChannelCallbackHandler);
                Internal.VyinChatEditor.GetCoroutineRunner().StartCoroutine(coroutine);
                return;
            }
#endif

            if (Application.platform == RuntimePlatform.Android)
            {
                try
                {
                    AndroidJavaClass androidBridge = new AndroidJavaClass("com.gamania.gim.unitybridge.UnityBridge");
                    AndroidJavaObject paramsObj = inChannelCreateParams.ToAndroidJavaObject();
                    var proxy = new GroupChannelCallbackProxy(inGroupChannelCallbackHandler);
                    androidBridge.CallStatic("createChannel", paramsObj, proxy);
                }
                catch (System.Exception e)
                {
                    inGroupChannelCallbackHandler?.Invoke(null, e.Message);
                }
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                try
                {
                    Internal.ChatSDKWrapper.CreateGroupChannel(inChannelCreateParams, inGroupChannelCallbackHandler);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[VyinChat] Error calling iOS CreateGroupChannel: " + e);
                    inGroupChannelCallbackHandler?.Invoke(null, e.Message);
                }
#else
                inGroupChannelCallbackHandler?.Invoke(null, "iOS SDK not available in this build");
#endif
            }
            else
            {
                inGroupChannelCallbackHandler?.Invoke(null, "Platform not supported");
            }
        }

        public static void CreateGroupChannel(
            VcGroupChannelCreateParams channelCreateParams,
            System.Action<string, string> callback)
        {
            if (channelCreateParams == null)
            {
                callback?.Invoke(null, "channelCreateParams is null");
                return;
            }

            string channelName = channelCreateParams.Name;
            string[] userIds = (channelCreateParams.UserIds ?? new List<string>()).ToArray();

#if UNITY_EDITOR
            if (Application.isEditor)
            {
                Debug.Log($"[VyinChat] Simulate CreateGroupChannel in Editor, name={channelName}, users={userIds.Length}");
                EditorApplication.delayCall += () =>
                {
                    string fakeResult = $"{{\"channelUrl\":\"channel_editor_123\",\"name\":\"{channelName}\"}}";
                    callback?.Invoke(fakeResult, null);
                };
                return;
            }
#endif

            // Unified handler for both platforms
            void handler(VcGroupChannel channel, string error)
            {
                if (!string.IsNullOrEmpty(error))
                {
                    callback?.Invoke(null, error);
                    Debug.LogError("[VyinChat] Error calling CreateGroupChannel: " + error);
                    return;
                }
                string channelUrl = channel?.ChannelUrl;
                string channelNameResult = channel?.Name;
                string result = $"{{\"channelUrl\":\"{channelUrl}\",\"name\":\"{channelNameResult}\"}}";
                callback?.Invoke(result, null);
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                try
                {
                    CreateGroupChannel(channelCreateParams, handler);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[VyinChat] Error calling CreateGroupChannel: " + e);
                    callback?.Invoke(null, e.Message);
                }
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                try
                {
                    Internal.ChatSDKWrapper.CreateGroupChannel(channelCreateParams, handler);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[VyinChat] Error calling iOS CreateGroupChannel: " + e);
                    callback?.Invoke(null, e.Message);
                }
#else
                callback?.Invoke(null, "iOS SDK not available in this build");
#endif
            }
            else
            {
                callback?.Invoke(null, "Platform not supported");
            }
        }

        private class GroupChannelCallbackProxy : AndroidJavaProxy
        {
            private readonly VcGroupChannelCallbackHandler callback;
            public GroupChannelCallbackProxy(VcGroupChannelCallbackHandler callback)
                : base("com.gamania.gim.sdk.handler.GroupChannelCallbackHandler")
            {
                this.callback = callback;
            }

            void onResult(AndroidJavaObject channel, AndroidJavaObject exception)
            {
                Debug.Log("onResult: channel=" + channel + ", error=" + exception);
                var error = exception.GetErrorMessage();
                if (!string.IsNullOrEmpty(error))
                {
                    callback?.Invoke(null, error);
                    Debug.LogError("onResult: error=" + error);
                    return;
                }
                var groupChannel = channel.ToVcGroupChannel();
                Debug.Log("onResult: groupChannel=" + groupChannel);
                callback.Invoke(groupChannel, null);
            }
        }
    }
}