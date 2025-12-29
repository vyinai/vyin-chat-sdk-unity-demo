// -----------------------------------------------------------------------------
//
// Transport Connection State
//
// -----------------------------------------------------------------------------

namespace VyinChatSdk.Transport
{
    /// <summary>
    /// Connection state for Transport layer
    /// </summary>
    public enum TransportConnectionState
    {
        /// <summary>
        /// Transport is closed/disconnected
        /// </summary>
        Closed,

        /// <summary>
        /// Transport is connecting to server
        /// </summary>
        Connecting,

        /// <summary>
        /// Transport is connected but not authenticated
        /// </summary>
        Connected,

        /// <summary>
        /// Transport is connected and authenticated (LOGI received)
        /// </summary>
        Authenticated,

        /// <summary>
        /// Authentication failed
        /// </summary>
        AuthenticationFailed
    }
}
