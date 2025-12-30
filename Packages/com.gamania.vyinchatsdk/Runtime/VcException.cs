// VcException.cs
// Pure C# - No Unity dependencies (KMP-ready)
// Public API - Users can catch and handle VcException

using System;

namespace VyinChatSdk
{
    /// <summary>
    /// Base exception class for VyinChat SDK
    /// All SDK exceptions should inherit from this class
    /// 100% Pure C#, no Unity dependencies (KMP-ready)
    /// </summary>
    public class VcException : Exception
    {
        /// <summary>
        /// Error code for programmatic error handling
        /// </summary>
        public VcErrorCode ErrorCode { get; }

        /// <summary>
        /// Additional error details
        /// </summary>
        public string Details { get; }

        public VcException(VcErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public VcException(VcErrorCode errorCode, string message, string details)
            : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public VcException(VcErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public VcException(VcErrorCode errorCode, string message, string details, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public override string ToString()
        {
            var result = $"[VcException] Code={ErrorCode}, Message={Message}";
            if (!string.IsNullOrEmpty(Details))
            {
                result += $", Details={Details}";
            }
            if (InnerException != null)
            {
                result += $", InnerException={InnerException.Message}";
            }
            return result;
        }
    }

    /// <summary>
    /// Error codes for VyinChat SDK
    /// </summary>
    public enum VcErrorCode
    {
        // General errors (1000-1099)
        Unknown = 1000,
        InvalidParameter = 1001,
        InvalidState = 1002,
        NotInitialized = 1003,
        NotConnected = 1004,

        // Authentication errors (1100-1199)
        AuthenticationFailed = 1100,
        InvalidToken = 1101,
        TokenExpired = 1102,
        Unauthorized = 1103,

        // Network errors (1200-1299)
        NetworkError = 1200,
        ConnectionFailed = 1201,
        ConnectionTimeout = 1202,
        RequestFailed = 1203,

        // HTTP errors (1300-1399)
        HttpBadRequest = 1300,
        HttpUnauthorized = 1301,
        HttpForbidden = 1302,
        HttpNotFound = 1303,
        HttpInternalServerError = 1304,

        // Channel errors (1400-1499)
        ChannelNotFound = 1400,
        ChannelCreationFailed = 1401,
        InvalidChannelUrl = 1402,

        // Message errors (1500-1599)
        MessageSendFailed = 1500,
        InvalidMessage = 1501,
        MessageTooLong = 1502,

        // WebSocket errors (1600-1699)
        WebSocketConnectionFailed = 1600,
        WebSocketDisconnected = 1601,
        WebSocketUpgradeFailed = 1602,

        // Command Protocol errors (1700-1799)
        CommandSendFailed = 1700,
        CommandTimeout = 1701,
        AckTimeout = 1702,
        InvalidCommand = 1703
    }
}
