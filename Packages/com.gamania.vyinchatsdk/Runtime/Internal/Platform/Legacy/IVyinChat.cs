using VyinChatSdk;

namespace VyinChatSdk.Internal
{
    public interface IVyinChat
    {
        void Init(VcInitParams initParams);
        void Connect(string userId, string authToken, VcUserHandler callback);
    }

    // public interface IGroupChannel
    // {
    //     void CreateChannel(VcGroupChannelCreateParams inChannelCreateParams, VcGroupChannelCallbackHandler inCompletionHandler);
    // }
}