// -----------------------------------------------------------------------------
//
// HTTP Transport Interface
//
// -----------------------------------------------------------------------------

using System;
using System.Collections;

namespace VyinChatSdk.Transport
{
    /// <summary>
    /// HTTP Transport interface for non-realtime operations
    /// Handles REST API requests
    /// </summary>
    public interface IHttpTransport
    {
        /// <summary>
        /// Session key for authenticating HTTP requests
        /// </summary>
        string SessionKey { get; set; }

        /// <summary>
        /// Execute a GET request
        /// </summary>
        /// <param name="endpoint">API endpoint (relative path)</param>
        /// <param name="callback">Callback with response or error</param>
        IEnumerator Get(string endpoint, Action<string, string> callback);

        /// <summary>
        /// Execute a POST request
        /// </summary>
        /// <param name="endpoint">API endpoint (relative path)</param>
        /// <param name="jsonBody">JSON request body</param>
        /// <param name="callback">Callback with response or error</param>
        IEnumerator Post(string endpoint, string jsonBody, Action<string, string> callback);

        /// <summary>
        /// Execute a PUT request
        /// </summary>
        /// <param name="endpoint">API endpoint (relative path)</param>
        /// <param name="jsonBody">JSON request body</param>
        /// <param name="callback">Callback with response or error</param>
        IEnumerator Put(string endpoint, string jsonBody, Action<string, string> callback);

        /// <summary>
        /// Execute a DELETE request
        /// </summary>
        /// <param name="endpoint">API endpoint (relative path)</param>
        /// <param name="callback">Callback with response or error</param>
        IEnumerator Delete(string endpoint, Action<string, string> callback);
    }
}
