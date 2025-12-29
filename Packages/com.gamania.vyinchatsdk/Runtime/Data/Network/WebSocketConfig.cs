// -----------------------------------------------------------------------------
//
// WebSocket Configuration
// Based on Swift SDK GIMConnection.getSocketPath()
//
// -----------------------------------------------------------------------------

using System;

namespace Gamania.VyinChatSDK.Data.Network
{
    /// <summary>
    /// WebSocket connection configuration
    /// </summary>
    public class WebSocketConfig
    {
        /// <summary>
        /// Application ID (e.g., "adb53e88-4c35-469a-a888-9e49ef1641b2")
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// User ID for authentication
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Access token for authentication
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Environment domain (default: "gamania.chat")
        /// </summary>
        public string EnvironmentDomain { get; set; } = "gamania.chat";

        /// <summary>
        /// App version (optional)
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// SDK version
        /// </summary>
        public string SdkVersion { get; set; } = "0.1.0";

        /// <summary>
        /// Connection timeout in seconds (default: 10)
        /// </summary>
        public float ConnectionTimeout { get; set; } = 10f;

        /// <summary>
        /// Build WebSocket URL
        /// Format: wss://{appId}.{domain}/ws?user_id=xxx&access_token=yyy&...
        /// Based on Swift SDK: GIMConnection.getSocketPath()
        /// </summary>
        public string BuildWebSocketUrl()
        {
            if (string.IsNullOrEmpty(ApplicationId))
            {
                throw new ArgumentException("ApplicationId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(UserId))
            {
                throw new ArgumentException("UserId cannot be null or empty");
            }

            // Build host: wss://{appId}.{domain}/ws
            string host = $"wss://{ApplicationId}.{EnvironmentDomain}/ws";

            // Build query parameters (matching Swift SDK)
            var queryParams = new System.Collections.Generic.List<string>
            {
                $"p=Unity",  // Platform
                $"pv={UnityEngine.Application.unityVersion}",  // Platform version
                $"sv={Uri.EscapeDataString(SdkVersion)}",  // SDK version
                $"ai={Uri.EscapeDataString(ApplicationId)}",  // Application ID
                $"user_id={Uri.EscapeDataString(UserId)}"  // User ID
            };

            // Add optional access token
            if (!string.IsNullOrEmpty(AccessToken))
            {
                queryParams.Add($"access_token={Uri.EscapeDataString(AccessToken)}");
            }

            // Add optional app version
            if (!string.IsNullOrEmpty(AppVersion))
            {
                queryParams.Add($"av={Uri.EscapeDataString(AppVersion)}");
            }

            // Combine host + query string
            return $"{host}?{string.Join("&", queryParams)}";
        }
    }
}
