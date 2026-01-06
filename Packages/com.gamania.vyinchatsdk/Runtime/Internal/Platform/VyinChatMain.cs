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
        private IChannelRepository _channelRepository;
        private string _baseUrl;
        private bool _isInitialized;
        private string _appId;

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
        }

        public void Init(VcInitParams initParams)
        {
            if (initParams == null)
            {
                Debug.LogError("[VyinChatMain] Init failed: initParams is null");
                return;
            }

            _appId = initParams.AppId;
            _isInitialized = true;
            Debug.Log($"[VyinChatMain] Initializing with AppId: {_appId}");
        }

        public void Connect(string userId, string authToken, string apiHost, string wsHost, VcUserHandler callback)
        {
            Debug.Log($"[VyinChatMain] isInitialized={_isInitialized}");

            if (!_isInitialized)
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
            apiHost = string.IsNullOrWhiteSpace(apiHost) ? GetDefaultApiHost(_appId) : apiHost;
            wsHost = string.IsNullOrWhiteSpace(wsHost) ? GetDefaultWsHost(_appId) : wsHost;

            Debug.Log($"[VyinChatMain] Connecting with API host: {apiHost}, WS host: {wsHost}");

            // TODO: Implement pure C# WebSocket connection to wsHost
            // On successful WebSocket connection:
            // 1. Extract session_key from WebSocket response
            // 2. Call SetSessionKey(sessionKey) to enable authenticated HTTP requests

            // Initialize HTTP repositories with API host
            _baseUrl = apiHost;
            _channelRepository = new ChannelRepositoryImpl(_httpClient, _baseUrl);
            Debug.Log($"[VyinChatMain] HTTP repositories initialized with API host: {_baseUrl}");

            // For now, WebSocket connection is not implemented
            Debug.LogWarning("[VyinChatMain] WebSocket Connect not yet implemented in Pure C# mode");
            callback?.Invoke(null, "Pure C# WebSocket Connect not yet implemented");
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
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    "VyinChatMain not initialized. Call Init() first.");
            }
        }

        /// <summary>
        /// Reset instance (for testing)
        /// </summary>
        public static void Reset()
        {
            _instance = null;
        }
    }
}
