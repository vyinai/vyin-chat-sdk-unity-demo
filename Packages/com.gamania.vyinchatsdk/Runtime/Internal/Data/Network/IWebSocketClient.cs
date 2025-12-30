using System;

namespace VyinChatSdk.Internal.Data.Network
{
    /// <summary>
    /// WebSocket client interface for Data layer
    /// Platform-agnostic interface following Clean Architecture principles
    /// </summary>
    public interface IWebSocketClient
    {
        /// <summary>
        /// Event triggered when WebSocket connection is established
        /// </summary>
        event Action OnConnected;

        /// <summary>
        /// Event triggered when WebSocket connection is closed
        /// </summary>
        event Action OnDisconnected;

        /// <summary>
        /// Event triggered when a message is received
        /// </summary>
        event Action<string> OnMessageReceived;

        /// <summary>
        /// Event triggered when an error occurs
        /// </summary>
        event Action<string> OnError;

        /// <summary>
        /// Current connection state
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connect to WebSocket server with configuration
        /// </summary>
        /// <param name="config">WebSocket connection configuration</param>
        void Connect(WebSocketConfig config);

        /// <summary>
        /// Disconnect from WebSocket server
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Send a message through WebSocket
        /// </summary>
        /// <param name="message">Message to send</param>
        void Send(string message);

        /// <summary>
        /// Update method to process WebSocket events (call from Unity Update loop)
        /// </summary>
        void Update();
    }
}
