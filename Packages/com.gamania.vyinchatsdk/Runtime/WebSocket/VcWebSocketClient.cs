// -----------------------------------------------------------------------------
//
// WebSocket Client Implementation (using WebSocketSharp)
//
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace VyinChatSdk.WebSocket
{
    /// <summary>
    /// WebSocket client implementation using websocket-sharp
    /// </summary>
    public class VcWebSocketClient : IVcWebSocket
    {
        private WebSocketSharp.WebSocket webSocket;
        private VcWebSocketConnectionState currentState;
        private readonly object stateLock = new object();

        public VcWebSocketConnectionState State
        {
            get
            {
                lock (stateLock)
                {
                    return currentState;
                }
            }
            private set
            {
                lock (stateLock)
                {
                    if (currentState != value)
                    {
                        currentState = value;
                        Debug.Log($"[VcWebSocket] State changed to: {currentState}");

                        // Invoke on Unity main thread
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            OnStateChanged?.Invoke(currentState);
                        });
                    }
                }
            }
        }

        public event Action<VcWebSocketConnectionState> OnStateChanged;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnError;

        public VcWebSocketClient()
        {
            currentState = VcWebSocketConnectionState.Closed;
        }

        public void Connect(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                if (State != VcWebSocketConnectionState.Closed)
                {
                    Debug.LogWarning("[VcWebSocket] Already connected or connecting");
                    return;
                }

                State = VcWebSocketConnectionState.Connecting;

                webSocket = new WebSocketSharp.WebSocket(url);

                // Add headers if provided
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        webSocket.SetCookie(new WebSocketSharp.Net.Cookie(header.Key, header.Value));
                    }
                }

                // Register event handlers
                webSocket.OnOpen += OnWebSocketOpen;
                webSocket.OnMessage += OnWebSocketMessage;
                webSocket.OnError += OnWebSocketError;
                webSocket.OnClose += OnWebSocketClose;

                // Connect
                webSocket.ConnectAsync();

                Debug.Log($"[VcWebSocket] Connecting to {url}...");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VcWebSocket] Connection error: {ex.Message}");
                State = VcWebSocketConnectionState.Closed;
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnError?.Invoke(ex.Message);
                });
            }
        }

        public void Disconnect()
        {
            try
            {
                if (webSocket == null || State == VcWebSocketConnectionState.Closed)
                {
                    Debug.Log("[VcWebSocket] Already disconnected");
                    return;
                }

                Debug.Log("[VcWebSocket] Disconnecting...");

                if (webSocket.ReadyState == WebSocketState.Open ||
                    webSocket.ReadyState == WebSocketState.Connecting)
                {
                    webSocket.CloseAsync();
                }
                else
                {
                    CleanupWebSocket();
                    State = VcWebSocketConnectionState.Closed;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VcWebSocket] Disconnect error: {ex.Message}");
                State = VcWebSocketConnectionState.Closed;
            }
        }

        public void Send(string message)
        {
            try
            {
                if (webSocket == null || State != VcWebSocketConnectionState.Open)
                {
                    string errorMsg = "Cannot send message: WebSocket is not connected";
                    Debug.LogWarning($"[VcWebSocket] {errorMsg}");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnError?.Invoke(errorMsg);
                    });
                    return;
                }

                webSocket.SendAsync(message, (completed) =>
                {
                    if (completed)
                    {
                        Debug.Log($"[VcWebSocket] Message sent: {message}");
                    }
                    else
                    {
                        Debug.LogError($"[VcWebSocket] Failed to send message");
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            OnError?.Invoke("Failed to send message");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VcWebSocket] Send error: {ex.Message}");
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnError?.Invoke(ex.Message);
                });
            }
        }

        #region WebSocketSharp Event Handlers

        private void OnWebSocketOpen(object sender, EventArgs e)
        {
            Debug.Log("[VcWebSocket] Connected");
            State = VcWebSocketConnectionState.Open;
        }

        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                Debug.Log($"[VcWebSocket] Message received: {e.Data}");
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnMessageReceived?.Invoke(e.Data);
                });
            }
        }

        private void OnWebSocketError(object sender, ErrorEventArgs e)
        {
            Debug.LogError($"[VcWebSocket] Error: {e.Message}");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnError?.Invoke(e.Message);
            });
        }

        private void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            Debug.Log($"[VcWebSocket] Disconnected (Code: {e.Code}, Reason: {e.Reason})");
            CleanupWebSocket();
            State = VcWebSocketConnectionState.Closed;
        }

        #endregion

        private void CleanupWebSocket()
        {
            if (webSocket != null)
            {
                webSocket.OnOpen -= OnWebSocketOpen;
                webSocket.OnMessage -= OnWebSocketMessage;
                webSocket.OnError -= OnWebSocketError;
                webSocket.OnClose -= OnWebSocketClose;
                webSocket = null;
            }
        }
    }
}
