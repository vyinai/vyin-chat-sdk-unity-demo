// -----------------------------------------------------------------------------
//
// Unity HTTP Client - Task 3.2
// Concrete implementation using UnityWebRequest
// Supports all Unity platforms including WebGL, iOS, Android
//
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VyinChatSdk.Internal.Data.Network;

namespace VyinChatSdk.Internal.Platform.Unity.Network
{
    /// <summary>
    /// Unity HTTP client implementation using UnityWebRequest
    /// Implements IHttpClient for platform-independent HTTP communication
    /// </summary>
    public class UnityHttpClient : IHttpClient
    {
        private string _sessionKey;
        private const string SessionKeyHeader = "Session-Key";
        private const int DefaultTimeoutSeconds = 30;

        /// <summary>
        /// Sets the session key for authenticated requests
        /// </summary>
        public void SetSessionKey(string sessionKey)
        {
            _sessionKey = sessionKey;
        }

        /// <summary>
        /// Performs a GET request
        /// </summary>
        public async Task<HttpResponse> GetAsync(
            string url,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                return await SendRequestAsync(request, headers, cancellationToken);
            }
        }

        /// <summary>
        /// Performs a POST request
        /// </summary>
        public async Task<HttpResponse> PostAsync(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            using (var request = new UnityWebRequest(url, "POST"))
            {
                // Set request body
                if (!string.IsNullOrEmpty(body))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                request.downloadHandler = new DownloadHandlerBuffer();

                return await SendRequestAsync(request, headers, cancellationToken);
            }
        }

        /// <summary>
        /// Performs a PUT request
        /// </summary>
        public async Task<HttpResponse> PutAsync(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            using (var request = new UnityWebRequest(url, "PUT"))
            {
                // Set request body
                if (!string.IsNullOrEmpty(body))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                request.downloadHandler = new DownloadHandlerBuffer();

                return await SendRequestAsync(request, headers, cancellationToken);
            }
        }

        /// <summary>
        /// Performs a DELETE request
        /// </summary>
        public async Task<HttpResponse> DeleteAsync(
            string url,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            using (var request = UnityWebRequest.Delete(url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                return await SendRequestAsync(request, headers, cancellationToken);
            }
        }

        /// <summary>
        /// Sends the HTTP request and converts UnityWebRequest to HttpResponse
        /// Handles session key injection, error handling, and timeout
        /// </summary>
        private async Task<HttpResponse> SendRequestAsync(
            UnityWebRequest request,
            Dictionary<string, string> headers,
            CancellationToken cancellationToken)
        {
            try
            {
                // Set timeout
                request.timeout = DefaultTimeoutSeconds;

                // Add session key header if available
                if (!string.IsNullOrEmpty(_sessionKey))
                {
                    request.SetRequestHeader(SessionKeyHeader, _sessionKey);
                }

                // Add custom headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }

                // Send request asynchronously
                var asyncOperation = request.SendWebRequest();

                // Wait for completion with cancellation support
                while (!asyncOperation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        request.Abort();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Yield();
                }

                // Build response
                var response = new HttpResponse
                {
                    StatusCode = (int)request.responseCode,
                    Body = request.downloadHandler?.text ?? string.Empty
                };

                // Copy response headers
                var responseHeaders = request.GetResponseHeaders();
                if (responseHeaders != null)
                {
                    foreach (var header in responseHeaders)
                    {
                        response.Headers[header.Key] = header.Value;
                    }
                }

                // Handle errors
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    response.Error = request.error;
                }

                return response;
            }
            catch (OperationCanceledException)
            {
                // Re-throw cancellation
                throw;
            }
            catch (Exception ex)
            {
                // Wrap unexpected errors
                return new HttpResponse
                {
                    StatusCode = 0,
                    Error = $"HTTP request failed: {ex.Message}",
                    Body = string.Empty
                };
            }
        }
    }
}
