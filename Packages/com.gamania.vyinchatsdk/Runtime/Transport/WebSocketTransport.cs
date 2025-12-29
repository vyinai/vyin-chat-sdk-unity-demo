// -----------------------------------------------------------------------------
//
// WebSocket Transport Implementation
//
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using VyinChatSdk.WebSocket;
using VyinChatSdk.Transport.Protocol;

namespace VyinChatSdk.Transport
{
    /// <summary>
    /// WebSocket Transport for real-time messaging
    /// Handles connection, LOGI authentication, and command transmission
    /// </summary>
    public class WebSocketTransport : IWebSocketTransport
    {
        private readonly IVcWebSocket _webSocket;
        private readonly string _appId;
        private readonly string _domain;
        private TransportConnectionState _state;
        private string _sessionKey;
        private string _userId;
        private string _accessToken;
        private float _logiTimeoutSeconds = 10f;
        private float _logiStartTime;
        private bool _waitingForLogi;

        public TransportConnectionState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    Debug.Log($"[WebSocketTransport] State changed to: {_state}");
                    OnStateChanged?.Invoke(_state);
                }
            }
        }

        public string SessionKey => _sessionKey;

        public event Action<TransportConnectionState> OnStateChanged;
        public event Action<string> OnAuthenticated;
        public event Action<string> OnAuthenticationFailed;
        public event Action<string> OnCommandReceived;
        public event Action<string> OnError;

        public WebSocketTransport(string appId, string domain, IVcWebSocket webSocket = null)
        {
            _appId = appId;
            _domain = domain;
            _webSocket = webSocket ?? new VcWebSocketClient();
            _state = TransportConnectionState.Closed;

            // Subscribe to WebSocket events
            _webSocket.OnStateChanged += OnWebSocketStateChanged;
            _webSocket.OnMessageReceived += OnWebSocketMessageReceived;
            _webSocket.OnError += OnWebSocketError;
        }

        public void Connect(string userId, string accessToken)
        {
            if (State != TransportConnectionState.Closed)
            {
                Debug.LogWarning("[WebSocketTransport] Already connected or connecting");
                return;
            }

            _userId = userId;
            _accessToken = accessToken;
            _sessionKey = null;
            _waitingForLogi = false;

            State = TransportConnectionState.Connecting;

            // Build WebSocket URL with query parameters
            string wsUrl = BuildWebSocketUrl(userId, accessToken);
            Debug.Log($"[WebSocketTransport] Connecting to: {wsUrl}");

            _webSocket.Connect(wsUrl);
        }

        public void Disconnect()
        {
            if (State == TransportConnectionState.Closed)
            {
                Debug.Log("[WebSocketTransport] Already disconnected");
                return;
            }

            Debug.Log("[WebSocketTransport] Disconnecting...");
            _waitingForLogi = false;
            _webSocket.Disconnect();
            State = TransportConnectionState.Closed;
        }

        public void SendCommand(string command)
        {
            if (State != TransportConnectionState.Authenticated)
            {
                string error = "Cannot send command: not authenticated";
                Debug.LogWarning($"[WebSocketTransport] {error}");
                OnError?.Invoke(error);
                return;
            }

            _webSocket.Send(command);
        }

        /// <summary>
        /// Update method to handle LOGI timeout
        /// Should be called from MonoBehaviour Update
        /// </summary>
        public void Update()
        {
            if (_waitingForLogi)
            {
                float elapsed = Time.time - _logiStartTime;
                if (elapsed > _logiTimeoutSeconds)
                {
                    Debug.LogError("[WebSocketTransport] LOGI timeout");
                    _waitingForLogi = false;
                    OnAuthenticationFailed?.Invoke("LOGI timeout - no response within 10 seconds");
                    Disconnect();
                }
            }
        }

        private string BuildWebSocketUrl(string userId, string accessToken)
        {
            string baseUrl = $"wss://{_appId}.{_domain}/ws";

            var queryParams = new Dictionary<string, string>
            {
                { "user_id", UnityEngine.Networking.UnityWebRequest.EscapeURL(userId) },
                { "access_token", UnityEngine.Networking.UnityWebRequest.EscapeURL(accessToken ?? "") },
                { "active", "1" }
            };

            var queryString = string.Join("&",
                System.Linq.Enumerable.Select(queryParams, kvp => $"{kvp.Key}={kvp.Value}"));

            return $"{baseUrl}?{queryString}";
        }

        private void OnWebSocketStateChanged(VcWebSocketConnectionState wsState)
        {
            Debug.Log($"[WebSocketTransport] WebSocket state: {wsState}");

            switch (wsState)
            {
                case VcWebSocketConnectionState.Open:
                    State = TransportConnectionState.Connected;
                    SendLogiCommand();
                    break;

                case VcWebSocketConnectionState.Closed:
                    if (State != TransportConnectionState.Closed)
                    {
                        State = TransportConnectionState.Closed;
                    }
                    break;
            }
        }

        private void SendLogiCommand()
        {
            Debug.Log("[WebSocketTransport] Sending LOGI command...");

            var logiCommand = new LogiCommand
            {
                UserId = _userId,
                AccessToken = _accessToken ?? "",
                ReqId = RequestIdGenerator.Generate()
            };

            string commandString = logiCommand.Serialize();
            Debug.Log($"[WebSocketTransport] LOGI: {commandString}");

            _webSocket.Send(commandString);

            // Start LOGI timeout timer
            _waitingForLogi = true;
            _logiStartTime = Time.time;
        }

        private void OnWebSocketMessageReceived(string message)
        {
            Debug.Log($"[WebSocketTransport] Message received: {message}");

            string commandType = CommandParser.GetCommandType(message);

            if (commandType == "LOGI")
            {
                HandleLogiResponse(message);
            }
            else
            {
                // Forward other commands to upper layer
                OnCommandReceived?.Invoke(message);
            }
        }

        private void HandleLogiResponse(string message)
        {
            Debug.Log("[WebSocketTransport] Handling LOGI response...");

            _waitingForLogi = false;

            var logiResponse = CommandParser.ParseLogiResponse(message);

            if (logiResponse == null)
            {
                Debug.LogError("[WebSocketTransport] Failed to parse LOGI response");
                OnAuthenticationFailed?.Invoke("Invalid LOGI response format");
                Disconnect();
                return;
            }

            if (!string.IsNullOrEmpty(logiResponse.Error))
            {
                Debug.LogError($"[WebSocketTransport] LOGI error: {logiResponse.Error}");
                OnAuthenticationFailed?.Invoke(logiResponse.Error);
                Disconnect();
                return;
            }

            if (string.IsNullOrEmpty(logiResponse.SessionKey))
            {
                Debug.LogError("[WebSocketTransport] LOGI response missing session key");
                OnAuthenticationFailed?.Invoke("Missing session key in LOGI response");
                Disconnect();
                return;
            }

            // Success
            _sessionKey = logiResponse.SessionKey;
            Debug.Log($"[WebSocketTransport] Authentication successful, session key: {_sessionKey}");
            State = TransportConnectionState.Authenticated;
            OnAuthenticated?.Invoke(_sessionKey);
        }

        private void OnWebSocketError(string error)
        {
            Debug.LogError($"[WebSocketTransport] WebSocket error: {error}");
            OnError?.Invoke(error);
        }
    }
}
