// Tests/Runtime/Platform/MockHttpClient.cs
// Mock HTTP Client for testing

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VyinChatSdk.Internal.Data.Network;

namespace VyinChatSdk.Tests.Mocks.Platform
{
    /// <summary>
    /// Mock HTTP Client for testing Phase 3
    /// Allows you to develop without waiting for Phase 1-2
    /// </summary>
    public class MockHttpClient : IHttpClient
    {
        private readonly Queue<HttpResponse> _responseQueue = new Queue<HttpResponse>();
        private HttpResponse _defaultResponse;

        // For verification in tests
        public List<(string method, string url, string body)> RequestHistory { get; } = new List<(string, string, string)>();

        /// <summary>
        /// Queue a response to be returned on next request
        /// </summary>
        public void QueueResponse(HttpResponse response)
        {
            _responseQueue.Enqueue(response);
        }

        /// <summary>
        /// Set default response for all requests
        /// </summary>
        public void SetDefaultResponse(HttpResponse response)
        {
            _defaultResponse = response;
        }

        /// <summary>
        /// Clear all queued responses and history
        /// </summary>
        public void Reset()
        {
            _responseQueue.Clear();
            RequestHistory.Clear();
            _defaultResponse = null;
        }

        public Task<HttpResponse> GetAsync(
            string url,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            RequestHistory.Add(("GET", url, null));
            return Task.FromResult(GetNextResponse());
        }

        public Task<HttpResponse> PostAsync(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            RequestHistory.Add(("POST", url, body));
            return Task.FromResult(GetNextResponse());
        }

        public Task<HttpResponse> PutAsync(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            RequestHistory.Add(("PUT", url, body));
            return Task.FromResult(GetNextResponse());
        }

        public Task<HttpResponse> DeleteAsync(
            string url,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
        {
            RequestHistory.Add(("DELETE", url, null));
            return Task.FromResult(GetNextResponse());
        }

        private HttpResponse GetNextResponse()
        {
            if (_responseQueue.Count > 0)
            {
                return _responseQueue.Dequeue();
            }

            if (_defaultResponse != null)
            {
                return _defaultResponse;
            }

            // Default success response
            return new HttpResponse
            {
                StatusCode = 200,
                Body = "{}",
                Headers = new Dictionary<string, string>()
            };
        }
    }

    /// <summary>
    /// Helper class to build HttpResponse for tests
    /// </summary>
    public static class MockHttpResponseBuilder
    {
        public static HttpResponse Success(string body = "{}")
        {
            return new HttpResponse
            {
                StatusCode = 200,
                Body = body,
                Headers = new Dictionary<string, string>()
            };
        }

        public static HttpResponse NotFound(string body = "{\"error\":\"Not found\"}")
        {
            return new HttpResponse
            {
                StatusCode = 404,
                Body = body,
                Error = "Not found"
            };
        }

        public static HttpResponse Unauthorized(string body = "{\"error\":\"Unauthorized\"}")
        {
            return new HttpResponse
            {
                StatusCode = 401,
                Body = body,
                Error = "Unauthorized"
            };
        }

        public static HttpResponse ServerError(string body = "{\"error\":\"Internal server error\"}")
        {
            return new HttpResponse
            {
                StatusCode = 500,
                Body = body,
                Error = "Internal server error"
            };
        }
    }
}
