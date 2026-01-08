using UnityEngine;
using VyinChatSdk;
using VyinChatSdk.Internal.Platform;

namespace VyinChatSdk.Internal
{
    internal class VyinChatAndroid : IVyinChat
    {
        private AndroidJavaClass androidBridge;

        public VyinChatAndroid()
        {
            try
            {
                androidBridge = new AndroidJavaClass("com.gamania.gim.unitybridge.UnityBridge");
                Debug.Log("[VyinChatAndroid] AndroidUnityBridge initialized");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[VyinChatAndroid] Failed to init: " + e.Message);
            }
        }

        public void Init(VcInitParams initParams)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            androidBridge.CallStatic("init", activity, initParams.AppId);
        }

        public void Connect(string userId, string authToken, string apiHost, string wsHost, VcUserHandler callback)
        {
            Debug.Log("[VyinChatAndroid] Connect userId:" + userId + ", authToken:" + authToken);
            var proxy = new ConnectCallbackProxy(callback);
            Debug.Log("Calling AndroidBridge.connect with proxy");
            androidBridge.CallStatic("connect", userId, authToken, apiHost, wsHost, proxy);
            Debug.Log("CallStatic connect finished");
        }

        private class ConnectCallbackProxy : AndroidJavaProxy
        {
            private readonly VcUserHandler callback;
            public ConnectCallbackProxy(VcUserHandler callback)
                : base("com.gamania.gim.sdk.handler.ConnectHandler")
            {
                this.callback = callback;
            }

            public void onConnected(AndroidJavaObject user, AndroidJavaObject exception)
            {
                Debug.Log("onConnected: user=" + user + ", error=" + exception);
                var error = exception.GetErrorMessage();
                var vcUser = !string.IsNullOrEmpty(error) ? null : user.ToVcUser();

                MainThreadDispatcher.Enqueue(() =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        callback?.Invoke(null, error);
                        Debug.LogError("onConnected: error=" + error);
                        return;
                    }
                    Debug.Log("onConnected: user=" + vcUser);
                    callback?.Invoke(vcUser, null);
                });
            }
        }
    }
}