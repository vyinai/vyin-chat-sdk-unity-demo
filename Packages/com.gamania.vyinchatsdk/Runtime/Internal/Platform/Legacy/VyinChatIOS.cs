using UnityEngine;
using VyinChatSdk;

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

        public void Connect(string userId, string authToken, VcUserHandler callback)
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
                    callback?.Invoke(user, error);
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