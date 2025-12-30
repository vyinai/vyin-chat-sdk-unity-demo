// -----------------------------------------------------------------------------
//
// Unity WebSocket Client - Integrated ACK Management
// Concrete implementation using NativeWebSocket library
// Supports all Unity platforms including WebGL, iOS, Android
// ACK management integrated similar to iOS SDK's GIMSocketManager
//
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using NativeWebSocket;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VyinChatSdk.Internal.Data.Network;
using VyinChatSdk.Internal.Domain.Commands;

namespace VyinChatSdk.Internal.Platform.Unity.Network
{
    /// <summary>
    /// Unity WebSocket client implementation using NativeWebSocket
    /// Supports WebGL, iOS, Android, and all Unity platforms
    /// Includes integrated ACK management
    /// </summary>
    public class UnityWebSocketClient : IWebSocketClient
    {
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<CommandType, string> OnCommandReceived;
        public event Action<string> OnAuthenticated;
        public event Action<string> OnError;

        public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;
        public string SessionKey => _sessionKey;

        private WebSocket _webSocket;
        private string _sessionKey;
        private CancellationTokenSource _authTimeoutCts;
        private readonly TimeSpan _authTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _defaultAckTimeout = TimeSpan.FromSeconds(5);

        // ACK management - similar to iOS SDK's ackHandlerMap
        private readonly Dictionary<string, PendingAck> _pendingAcks = new Dictionary<string, PendingAck>();
        private readonly object _ackLock = new object();
        private readonly ICommandProtocol _commandProtocol = new CommandProtocol();

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

            _sessionKey = null;

            // Set Unity platform version if not already set
            if (string.IsNullOrEmpty(config.PlatformVersion))
            {
                config.PlatformVersion = UnityEngine.Application.unityVersion;
            }

            StartAuthTimeout();

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
                CancelAuthTimeout();
                ClearAllPendingAcks();

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
        /// Send a command through WebSocket with ACK handling
        /// Similar to iOS SDK's sendTask(with:ackHandler:)
        /// </summary>
        public async Task<string> SendCommandAsync(
            CommandType commandType,
            object payload,
            TimeSpan? ackTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var (reqId, serialized) = _commandProtocol.BuildCommand(commandType, payload);

            // If command doesn't require ACK, send immediately and return
            if (!commandType.IsAckRequired())
            {
                SendRaw(serialized);
                return null;
            }

            // Create task completion source for ACK
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var timeout = ackTimeout ?? _defaultAckTimeout;
            timeoutCts.CancelAfter(timeout);

            // Register pending ACK - similar to iOS SDK's ackHandlerMap
            RegisterPendingAck(reqId, tcs, timeoutCts);

            try
            {
                SendRaw(serialized);
            }
            catch
            {
                // If send fails immediately, clean up pending ACK
                CompletePendingAck(reqId, null, cancelTimeout: true);
                throw;
            }

            // Register timeout callback
            timeoutCts.Token.Register(() =>
            {
                CompletePendingAck(reqId, null, cancelTimeout: false);
            });

            return await tcs.Task;
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
            CancelAuthTimeout();
            ClearAllPendingAcks();
            OnDisconnected?.Invoke();
        }

        private void HandleOnMessage(byte[] data)
        {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(data);
                Debug.Log($"[UnityWebSocketClient] Received: {message}");

                // Parse command type
                var commandType = CommandParser.ExtractCommandType(message);
                if (commandType == null)
                {
                    Debug.LogWarning($"[UnityWebSocketClient] Failed to parse command type from: {message}");
                    return;
                }

                var payload = CommandParser.ExtractPayload(message);

                // Handle LOGI / Authentication
                if (commandType == CommandType.LOGI)
                {
                    HandleLogiCommand(message, payload);
                }
                // Handle MACK - Acknowledgement
                else if (commandType == CommandType.MACK)
                {
                    HandleMackCommand(payload);
                }
                // Handle EROR - Error
                else if (commandType == CommandType.EROR)
                {
                    CancelAuthTimeout();
                    OnError?.Invoke("Authentication failed (EROR message).");
                    OnCommandReceived?.Invoke(commandType.Value, payload);
                }
                // All other commands - Dispatch as event
                else
                {
                    OnCommandReceived?.Invoke(commandType.Value, payload);
                }
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
            CancelAuthTimeout();
            OnError?.Invoke(errorMessage);
        }

        // ACK Management - similar to iOS SDK's ACK handling

        private void RegisterPendingAck(string reqId, TaskCompletionSource<string> tcs, CancellationTokenSource timeoutCts)
        {
            lock (_ackLock)
            {
                if (_pendingAcks.ContainsKey(reqId))
                {
                    throw new InvalidOperationException($"Duplicate reqId registration: {reqId}");
                }
                _pendingAcks.Add(reqId, new PendingAck(tcs, timeoutCts));
            }
        }

        private bool CompletePendingAck(string reqId, string ackPayload, bool cancelTimeout)
        {
            PendingAck ack;
            lock (_ackLock)
            {
                if (!_pendingAcks.TryGetValue(reqId, out ack))
                {
                    return false;
                }
                _pendingAcks.Remove(reqId);
            }

            if (cancelTimeout)
            {
                ack.TimeoutCts.Cancel();
            }

            ack.Tcs.TrySetResult(ackPayload);
            ack.Dispose();
            return true;
        }

        private void ClearAllPendingAcks()
        {
            lock (_ackLock)
            {
                foreach (var ack in _pendingAcks.Values)
                {
                    ack.TimeoutCts.Cancel();
                    ack.Tcs.TrySetCanceled();
                    ack.Dispose();
                }
                _pendingAcks.Clear();
            }
        }

        private void HandleMackCommand(string payload)
        {
            var reqId = ExtractReqId(payload);
            if (string.IsNullOrWhiteSpace(reqId))
            {
                Debug.LogWarning("[UnityWebSocketClient] MACK received without req_id");
                return;
            }

            bool completed = CompletePendingAck(reqId, payload, cancelTimeout: true);
            if (!completed)
            {
                Debug.LogWarning($"[UnityWebSocketClient] MACK received for unknown reqId: {reqId}");
            }
        }

        private void HandleLogiCommand(string message, string payload)
        {
            var logi = CommandParser.ParseLogiCommand(message);
            if (logi != null)
            {
                if (logi.IsSuccess())
                {
                    _sessionKey = logi.SessionKey;
                    CancelAuthTimeout();
                    OnAuthenticated?.Invoke(_sessionKey);
                }
                else
                {
                    CancelAuthTimeout();
                    OnError?.Invoke("Authentication failed (LOGI error).");
                }
            }
        }

        private static string ExtractReqId(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return null;
            }

            const string key = "\"req_id\":\"";
            var start = payload.IndexOf(key, StringComparison.Ordinal);
            if (start < 0)
            {
                return null;
            }

            start += key.Length;
            var end = payload.IndexOf('"', start);
            if (end < 0 || end <= start)
            {
                return null;
            }

            return payload.Substring(start, end - start);
        }

        private void SendRaw(string message)
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                string error = "Cannot send message: WebSocket is not connected";
                Debug.LogError($"[UnityWebSocketClient] {error}");
                OnError?.Invoke(error);
                throw new InvalidOperationException(error);
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
                throw;
            }
        }

        private void StartAuthTimeout()
        {
            CancelAuthTimeout();
            _authTimeoutCts = new CancellationTokenSource();
            var token = _authTimeoutCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_authTimeout, token);
                    if (token.IsCancellationRequested || !string.IsNullOrEmpty(_sessionKey))
                    {
                        return;
                    }
                    OnError?.Invoke("Authentication timeout (LOGI not received).");
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            }, token);
        }

        private void CancelAuthTimeout()
        {
            if (_authTimeoutCts != null)
            {
                _authTimeoutCts.Cancel();
                _authTimeoutCts.Dispose();
                _authTimeoutCts = null;
            }
        }

        private sealed class PendingAck : IDisposable
        {
            public TaskCompletionSource<string> Tcs { get; }
            public CancellationTokenSource TimeoutCts { get; }

            public PendingAck(TaskCompletionSource<string> tcs, CancellationTokenSource timeoutCts)
            {
                Tcs = tcs;
                TimeoutCts = timeoutCts;
            }

            public void Dispose()
            {
                TimeoutCts.Dispose();
            }
        }
    }
}
