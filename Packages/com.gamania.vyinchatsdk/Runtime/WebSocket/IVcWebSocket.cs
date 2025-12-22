// -----------------------------------------------------------------------------
//
// WebSocket Interface
//
// -----------------------------------------------------------------------------

using System;

namespace VyinChatSdk.WebSocket
{
    /// <summary>
    /// WebSocket client interface
    /// </summary>
    public interface IVcWebSocket
    {
        /// <summary>
        /// Current connection state
        /// </summary>
        VcWebSocketConnectionState State { get; }

        /// <summary>
        /// Event fired when connection state changes
        /// </summary>
        event Action<VcWebSocketConnectionState> OnStateChanged;

        /// <summary>
        /// Event fired when a message is received
        /// </summary>
        event Action<string> OnMessageReceived;

        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        event Action<string> OnError;

        /// <summary>
        /// Connect to WebSocket server
        /// </summary>
        /// <param name="url">WebSocket server URL</param>
        /// <param name="headers">Optional headers</param>
        void Connect(string url, System.Collections.Generic.Dictionary<string, string> headers = null);

        /// <summary>
        /// Disconnect from WebSocket server
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Send a text message
        /// </summary>
        /// <param name="message">Message to send</param>
        void Send(string message);
    }
}
