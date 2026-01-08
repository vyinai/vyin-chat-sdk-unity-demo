using UnityEngine;
using VyinChatSdk;
using VyinChatSdk.Internal.Platform;

namespace VyinChatSdk.Internal
{
    internal class VyinChatIOS : IVyinChat
    {
        public VyinChatIOS()
        {
        }

        public void Init(VcInitParams initParams)
        {
            try
            {
                ChatSDKWrapper.Initialize(initParams.AppId);
                Debug.Log("[VyinChat] iOS ChatSDK initialized with AppId=" + initParams.AppId);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error calling iOS Initialize: " + e);
            }
        }

        public void Connect(string userId, string authToken, string apiHost, string wsHost, VcUserHandler callback)
        {
            try
            {
                ChatSDKWrapper.Connect(userId, authToken, (result, error) =>
                {
                    VcUser user = null;
                    if (!string.IsNullOrEmpty(result))
                    {
                        user = new VcUser { UserId = userId };
                    }

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        callback?.Invoke(user, error);
                    });
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error calling iOS Connect: " + e);
                callback?.Invoke(null, e.Message);
            }
        }
    }
}