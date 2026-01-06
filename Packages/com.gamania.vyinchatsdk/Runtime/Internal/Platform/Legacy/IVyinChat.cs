using VyinChatSdk;

namespace VyinChatSdk.Internal
{
    public interface IVyinChat
    {
        void Init(VcInitParams initParams);

        /// <summary>
        /// Connect to chat service
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="authToken">Optional auth token</param>
        /// <param name="apiHost">API host URL, null for platform default</param>
        /// <param name="wsHost">WebSocket host URL, null for platform default</param>
        /// <param name="callback">Callback with user or error</param>
        void Connect(string userId, string authToken, string apiHost, string wsHost, VcUserHandler callback);
    }

    // public interface IGroupChannel
    // {
    //     void CreateChannel(VcGroupChannelCreateParams inChannelCreateParams, VcGroupChannelCallbackHandler inCompletionHandler);
    // }
}