#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VyinChatSdk.Internal
{
    internal class VyinChatEditor : IVyinChat
    {
        public VyinChatEditor()
        {
            Debug.Log("VyinChat running in Editor, UnityBridge will be simulated.");
        }

        public void Init(VcInitParams initParams)
        {
            Debug.Log("Simulate Init in Editor with AppId=" + initParams.AppId);
        }

        public void Connect(string userId, string authToken, VcUserHandler callback)
        {
            Debug.Log("Simulate Connect in Editor, userId=" + userId);
            EditorApplication.delayCall += () =>
            {
                var user = new VcUser { UserId = "editor_" + userId, Nickname = "EditorUser" };
                Debug.Log("Simulated onConnectResult called, userId=" + user.UserId);
                callback?.Invoke(user, null);
            };
        }
    }
}
#endif