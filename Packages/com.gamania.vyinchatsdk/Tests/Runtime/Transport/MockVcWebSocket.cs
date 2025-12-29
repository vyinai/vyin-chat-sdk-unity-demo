// -----------------------------------------------------------------------------
//
// Mock WebSocket for Testing
//
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using VyinChatSdk.WebSocket;

namespace VyinChatSdk.Tests.Transport
{
    /// <summary>
    /// Mock WebSocket implementation for testing
    /// </summary>
    public class MockVcWebSocket : IVcWebSocket
    {
        public VcWebSocketConnectionState State { get; private set; }

        public event Action<VcWebSocketConnectionState> OnStateChanged;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnError;

        public string LastConnectedUrl { get; private set; }
        public Dictionary<string, string> LastConnectedHeaders { get; private set; }
        public List<string> SentMessages { get; private set; }

        public MockVcWebSocket()
        {
            State = VcWebSocketConnectionState.Closed;
            SentMessages = new List<string>();
        }

        public void Connect(string url, Dictionary<string, string> headers = null)
        {
            LastConnectedUrl = url;
            LastConnectedHeaders = headers;
            State = VcWebSocketConnectionState.Connecting;
            OnStateChanged?.Invoke(State);
        }

        public void Disconnect()
        {
            State = VcWebSocketConnectionState.Closed;
            OnStateChanged?.Invoke(State);
        }

        public void Send(string message)
        {
            SentMessages.Add(message);
        }

        // Test helpers
        public void SimulateConnected()
        {
            State = VcWebSocketConnectionState.Open;
            OnStateChanged?.Invoke(State);
        }

        public void SimulateMessage(string message)
        {
            OnMessageReceived?.Invoke(message);
        }

        public void SimulateError(string error)
        {
            OnError?.Invoke(error);
        }

        public void SimulateClosed()
        {
            State = VcWebSocketConnectionState.Closed;
            OnStateChanged?.Invoke(State);
        }
    }
}
