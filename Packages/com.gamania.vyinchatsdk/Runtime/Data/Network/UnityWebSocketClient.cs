// -----------------------------------------------------------------------------
//
// Unity WebSocket Client - Task 1.2
// Concrete implementation using NativeWebSocket library
// Supports all Unity platforms including WebGL, iOS, Android
//
// -----------------------------------------------------------------------------

using System;
using NativeWebSocket;
using UnityEngine;

namespace Gamania.VyinChatSDK.Data.Network
{
    /// <summary>
    /// Unity WebSocket client implementation using NativeWebSocket
    /// Supports WebGL, iOS, Android, and all Unity platforms
    /// </summary>
    public class UnityWebSocketClient : IWebSocketClient
    {
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnError;

        public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;

        private WebSocket _webSocket;
        private WebSocketConfig _config;

        /// <summary>
        /// Connect to WebSocket server with configuration
        /// </summary>
        public void Connect(WebSocketConfig config)
        {
            if (config == null)
            {
                OnError?.Invoke("WebSocketConfig cannot be null");
                return;
            }

            _config = config;

            // Build WSS URL using config
            string url;
            try
            {
                url = config.BuildWebSocketUrl();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Failed to build URL: {ex.Message}");
                return;
            }

            Debug.Log($"[UnityWebSocketClient] Connecting to {url}");

            try
            {
                _webSocket = new WebSocket(url);

                // Register event handlers
                _webSocket.OnOpen += HandleOnOpen;
                _webSocket.OnClose += HandleOnClose;
                _webSocket.OnMessage += HandleOnMessage;
                _webSocket.OnError += HandleOnError;

                // Start connection
                _ = _webSocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityWebSocketClient] Connect exception: {ex.Message}");
                OnError?.Invoke($"Connect failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnect from WebSocket server
        /// </summary>
        public void Disconnect()
        {
            if (_webSocket != null)
            {
                Debug.Log("[UnityWebSocketClient] Disconnecting");

                try
                {
                    _ = _webSocket.Close();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UnityWebSocketClient] Disconnect exception: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Send a message through WebSocket
        /// </summary>
        public void Send(string message)
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                string error = "Cannot send message: WebSocket is not connected";
                Debug.LogError($"[UnityWebSocketClient] {error}");
                OnError?.Invoke(error);
                return;
            }

            try
            {
                _ = _webSocket.SendText(message);
                Debug.Log($"[UnityWebSocketClient] Sent: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityWebSocketClient] Send exception: {ex.Message}");
                OnError?.Invoke($"Send failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Update method to process WebSocket events
        /// Must be called from Unity Update loop
        /// </summary>
        public void Update()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            _webSocket?.DispatchMessageQueue();
            #endif
        }

        // Event handlers

        private void HandleOnOpen()
        {
            Debug.Log("[UnityWebSocketClient] Connection opened");
            OnConnected?.Invoke();
        }

        private void HandleOnClose(WebSocketCloseCode closeCode)
        {
            Debug.Log($"[UnityWebSocketClient] Connection closed: {closeCode}");
            OnDisconnected?.Invoke();
        }

        private void HandleOnMessage(byte[] data)
        {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(data);
                Debug.Log($"[UnityWebSocketClient] Received: {message}");
                OnMessageReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityWebSocketClient] Message decode exception: {ex.Message}");
                OnError?.Invoke($"Failed to decode message: {ex.Message}");
            }
        }

        private void HandleOnError(string errorMessage)
        {
            Debug.LogError($"[UnityWebSocketClient] WebSocket error: {errorMessage}");
            OnError?.Invoke(errorMessage);
        }
    }
}
