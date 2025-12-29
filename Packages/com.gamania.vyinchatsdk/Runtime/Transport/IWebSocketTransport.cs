// -----------------------------------------------------------------------------
//
// WebSocket Transport Interface
//
// -----------------------------------------------------------------------------

using System;

namespace VyinChatSdk.Transport
{
    /// <summary>
    /// WebSocket Transport interface for real-time messaging
    /// Handles connection, authentication (LOGI), and message transmission
    /// </summary>
    public interface IWebSocketTransport
    {
        /// <summary>
        /// Current connection state
        /// </summary>
        TransportConnectionState State { get; }

        /// <summary>
        /// Session key obtained after LOGI authentication
        /// </summary>
        string SessionKey { get; }

        /// <summary>
        /// Event fired when connection state changes
        /// </summary>
        event Action<TransportConnectionState> OnStateChanged;

        /// <summary>
        /// Event fired when authentication succeeds (LOGI received)
        /// </summary>
        event Action<string> OnAuthenticated;

        /// <summary>
        /// Event fired when authentication fails
        /// </summary>
        event Action<string> OnAuthenticationFailed;

        /// <summary>
        /// Event fired when a command message is received
        /// </summary>
        event Action<string> OnCommandReceived;

        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        event Action<string> OnError;

        /// <summary>
        /// Connect to WebSocket server with user credentials
        /// </summary>
        /// <param name="userId">User ID for authentication</param>
        /// <param name="accessToken">Access token for authentication</param>
        void Connect(string userId, string accessToken);

        /// <summary>
        /// Disconnect from WebSocket server
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Send a command message
        /// </summary>
        /// <param name="command">Command string to send</param>
        void SendCommand(string command);
    }
}
