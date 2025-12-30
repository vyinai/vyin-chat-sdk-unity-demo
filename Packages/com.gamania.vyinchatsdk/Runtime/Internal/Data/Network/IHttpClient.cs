// Data/Network/IHttpClient.cs
// HTTP Client abstraction for platform independence

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VyinChatSdk.Internal.Data.Network
{
    /// <summary>
    /// HTTP Client interface for making REST API calls
    /// Platform abstraction to allow different implementations (UnityWebRequest, HttpClient, etc.)
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Performs a GET request
        /// </summary>
        /// <param name="url">Full URL to request</param>
        /// <param name="headers">Optional HTTP headers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>HTTP response</returns>
        Task<HttpResponse> GetAsync(
            string url,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a POST request
        /// </summary>
        /// <param name="url">Full URL to request</param>
        /// <param name="body">Request body (JSON string)</param>
        /// <param name="headers">Optional HTTP headers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>HTTP response</returns>
        Task<HttpResponse> PostAsync(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a PUT request
        /// </summary>
        Task<HttpResponse> PutAsync(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a DELETE request
        /// </summary>
        Task<HttpResponse> DeleteAsync(
            string url,
            Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// HTTP Response wrapper
    /// Pure C# data structure (KMP-ready)
    /// </summary>
    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
        public string Error { get; set; }

        public HttpResponse()
        {
            Headers = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// HTTP Client exception
    /// </summary>
    public class HttpClientException : Exception
    {
        public int StatusCode { get; }
        public string ResponseBody { get; }

        public HttpClientException(string message, int statusCode, string responseBody)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
