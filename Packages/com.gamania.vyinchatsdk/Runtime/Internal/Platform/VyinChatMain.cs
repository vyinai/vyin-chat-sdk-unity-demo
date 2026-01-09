using System;
using UnityEngine;
using VyinChatSdk.Internal.Data.Network;
using VyinChatSdk.Internal.Data.Repositories;
using VyinChatSdk.Internal.Domain.Repositories;
using VyinChatSdk.Internal.Platform.Unity.Network;

namespace VyinChatSdk.Internal.Platform
{
    internal class VyinChatMain : IVyinChat
    {
        private static VyinChatMain _instance;
        private IHttpClient _httpClient;
        private IWebSocketClient _webSocketClient;
        private IChannelRepository _channelRepository;
        private string _baseUrl;
        private VcInitParams _initParams;

        // Host configuration constants
        private const string API_HOST_PREFIX = "https://";
        private const string WS_HOST_PREFIX = "wss://";
        private const string HOST_POSTFIX = "gamania.chat";

        public static VyinChatMain Instance
        {
            get
            {
                _instance ??= new VyinChatMain();
                return _instance;
            }
        }

        public VyinChatMain()
        {
            _httpClient = new UnityHttpClient();
            _webSocketClient = new UnityWebSocketClient();
        }

        public void Init(VcInitParams initParams)
        {
            if (initParams == null)
            {
                throw new ArgumentNullException(nameof(initParams));
            }

            if (string.IsNullOrEmpty(initParams.AppId))
            {
                throw new ArgumentException("AppId cannot be null or empty", nameof(initParams));
            }

            _initParams = initParams;
            Debug.Log($"[VyinChatMain] Initialized with AppId: {initParams.AppId}, " +
                $"LocalCaching: {initParams.IsLocalCachingEnabled}, LogLevel: {initParams.LogLevel}");
        }

        public void Connect(string userId, string authToken, string apiHost, string wsHost, VcUserHandler callback)
        {
            Debug.Log($"[VyinChatMain] isInitialized={(_initParams != null)}");

            if (_initParams == null)
            {
                var errorMsg = "VyinChatMain instance hasn't been initialized. Try VyinChat.Init().";
                Debug.LogError($"[VyinChatMain] {errorMsg}");
                throw new InvalidOperationException(errorMsg);
            }

            if (string.IsNullOrEmpty(userId))
            {
                var errorMsg = "userId is empty.";
                Debug.LogError($"[VyinChatMain] {errorMsg}");
                callback?.Invoke(null, errorMsg);
                return;
            }

            ConnectInternal(userId, authToken, apiHost, wsHost, callback);
        }

        private void ConnectInternal(string userId, string authToken, string apiHost, string wsHost, VcUserHandler callback)
        {
            apiHost = string.IsNullOrWhiteSpace(apiHost) ? GetDefaultApiHost(_initParams.AppId) : apiHost;
            wsHost = string.IsNullOrWhiteSpace(wsHost) ? GetDefaultWsHost(_initParams.AppId) : wsHost;

            Debug.Log($"[VyinChatMain] Connecting with API host: {apiHost}, WS host: {wsHost}");

            // Initialize HTTP repositories with API host
            _baseUrl = apiHost;
            _channelRepository = new ChannelRepositoryImpl(_httpClient, _baseUrl);
            Debug.Log($"[VyinChatMain] HTTP repositories initialized with API host: {_baseUrl}");

            // Create WebSocket configuration
            var wsConfig = new WebSocketConfig
            {
                ApplicationId = _initParams.AppId,
                UserId = userId,
                AccessToken = authToken,
                AppVersion = _initParams.AppVersion,
                CustomWebSocketBaseUrl = wsHost
            };

            // Setup event handlers
            Action<string> onAuthenticatedHandler = null;
            Action<string> onErrorHandler = null;

            onAuthenticatedHandler = (sessionKey) =>
            {
                Debug.Log($"[VyinChatMain] Authentication successful, session key received");

                // Store session key for HTTP requests
                SetSessionKey(sessionKey);

                // Create user object
                var user = new VcUser
                {
                    UserId = userId
                };

                // Cleanup handlers
                _webSocketClient.OnAuthenticated -= onAuthenticatedHandler;
                _webSocketClient.OnError -= onErrorHandler;

                // Invoke success callback
                callback?.Invoke(user, null);
            };

            onErrorHandler = (error) =>
            {
                Debug.LogError($"[VyinChatMain] WebSocket error: {error}");

                // Cleanup handlers
                _webSocketClient.OnAuthenticated -= onAuthenticatedHandler;
                _webSocketClient.OnError -= onErrorHandler;

                // Invoke error callback
                callback?.Invoke(null, error);
            };

            _webSocketClient.OnAuthenticated += onAuthenticatedHandler;
            _webSocketClient.OnError += onErrorHandler;

            // Start WebSocket connection
            Debug.Log($"[VyinChatMain] Starting WebSocket connection");
            _webSocketClient.Connect(wsConfig);
        }

        private string GetDefaultApiHost(string appId)
        {
            return $"{API_HOST_PREFIX}{appId}.{HOST_POSTFIX}";
        }

        private string GetDefaultWsHost(string appId)
        {
            return $"{WS_HOST_PREFIX}{appId}.{HOST_POSTFIX}";
        }

        /// <summary>
        /// Set session key for authenticated HTTP requests
        /// Called after WebSocket connection establishes session
        /// </summary>
        public void SetSessionKey(string sessionKey)
        {
            if (_httpClient is UnityHttpClient unityHttpClient)
            {
                unityHttpClient.SetSessionKey(sessionKey);
                Debug.Log($"[VyinChatMain] Session key updated");
            }
        }

        /// <summary>
        /// Get Channel Repository instance
        /// </summary>
        public IChannelRepository GetChannelRepository()
        {
            EnsureInitialized();
            return _channelRepository;
        }

        /// <summary>
        /// Get HTTP Client instance
        /// </summary>
        public IHttpClient GetHttpClient()
        {
            return _httpClient;
        }

        private void EnsureInitialized()
        {
            if (_initParams == null)
            {
                throw new InvalidOperationException(
                    "VyinChatMain not initialized. Call Init() first.");
            }
        }

        /// <summary>
        /// Reset instance state (for testing)
        /// </summary>
        public void Reset()
        {
            // Disconnect WebSocket if connected
            if (_webSocketClient != null && _webSocketClient.IsConnected)
            {
                _webSocketClient.Disconnect();
            }

            _initParams = null;
            _httpClient = new UnityHttpClient();
            _webSocketClient = new UnityWebSocketClient();
            _channelRepository = null;
            _baseUrl = null;
        }
    }
}
