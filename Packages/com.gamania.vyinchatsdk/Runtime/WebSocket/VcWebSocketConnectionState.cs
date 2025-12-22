// -----------------------------------------------------------------------------
//
// WebSocket Connection State
//
// -----------------------------------------------------------------------------

namespace VyinChatSdk.WebSocket
{
    /// <summary>
    /// WebSocket connection state
    /// </summary>
    public enum VcWebSocketConnectionState
    {
        /// <summary>
        /// Connecting to server
        /// </summary>
        Connecting = 0,

        /// <summary>
        /// Connection established and open
        /// </summary>
        Open = 1,

        /// <summary>
        /// Connection closed
        /// </summary>
        Closed = 3
    }
}
