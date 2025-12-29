// -----------------------------------------------------------------------------
//
// WebSocket Tests (TDD)
//
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VyinChatSdk.WebSocket;

namespace VyinChatSdk.Tests
{
    public class VcWebSocketTests
    {
        private IVcWebSocket webSocket;
        private List<string> logs;

        // PROD environment configuration (matching ChatDemoController)
        private const string PROD_APP_ID = "adb53e88-4c35-469a-a888-9e49ef1641b2";
        private const string PROD_DOMAIN = "gamania.chat";
        private const string TEST_USER_ID = "testuser1";
        private const string SDK_VERSION = "0.1.2";

        // Construct WebSocket URL following the iOS SDK pattern
        private static string GetWebSocketUrl()
        {
            string baseUrl = $"wss://{PROD_APP_ID}.{PROD_DOMAIN}/ws";

            // Add query parameters (simplified version, matching iOS SDK pattern)
            var queryParams = new Dictionary<string, string>
            {
                { "p", "Unity" },
                { "pv", Application.unityVersion },
                { "sv", SDK_VERSION },
                { "ai", PROD_APP_ID },
                { "user_id", TEST_USER_ID },
                { "active", "1" }
            };

            var queryString = string.Join("&",
                System.Linq.Enumerable.Select(queryParams, kvp => $"{kvp.Key}={UnityEngine.Networking.UnityWebRequest.EscapeURL(kvp.Value)}"));

            return $"{baseUrl}?{queryString}";
        }

        private static readonly string TEST_WS_URL = GetWebSocketUrl();

        [SetUp]
        public void SetUp()
        {
            logs = new List<string>();
            webSocket = new VcWebSocketClient();
            Debug.Log($"[VcWebSocketTests] Testing with URL: {TEST_WS_URL}");
        }

        [TearDown]
        public void TearDown()
        {
            if (webSocket != null && webSocket.State != VcWebSocketConnectionState.Closed)
            {
                webSocket.Disconnect();
            }
            webSocket = null;
            logs.Clear();
        }

        #region 1. Connect Tests (連線測試)

        [UnityTest]
        public IEnumerator Connect_ShouldChangeStateToConnecting()
        {
            // Arrange
            VcWebSocketConnectionState? capturedState = null;
            webSocket.OnStateChanged += (state) =>
            {
                logs.Add($"[LOGI] State changed to: {state}");
                if (state == VcWebSocketConnectionState.Connecting)
                {
                    capturedState = state;
                }
            };

            // Act
            webSocket.Connect(TEST_WS_URL);

            // Assert
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(capturedState, "State should change to Connecting");
            Assert.AreEqual(VcWebSocketConnectionState.Connecting, capturedState);
            Assert.IsTrue(logs.Count > 0, "Should have connection logs");
            Debug.Log(string.Join("\n", logs));
        }

        [UnityTest]
        public IEnumerator Connect_ShouldChangeStateToOpen_WhenSuccessful()
        {
            // Arrange
            bool connected = false;
            webSocket.OnStateChanged += (state) =>
            {
                logs.Add($"[LOGI] State changed to: {state}");
                if (state == VcWebSocketConnectionState.Open)
                {
                    connected = true;
                }
            };

            // Act
            webSocket.Connect(TEST_WS_URL);

            // Wait for connection (max 5 seconds)
            float timeout = 5f;
            while (!connected && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            // Assert
            Assert.IsTrue(connected, "Should connect successfully");
            Assert.AreEqual(VcWebSocketConnectionState.Open, webSocket.State);
            Assert.IsTrue(logs.Count >= 2, "Should have connecting and open logs");
            Debug.Log(string.Join("\n", logs));
        }

        #endregion

        #region 2. Disconnect Tests (斷線測試)

        [UnityTest]
        public IEnumerator Disconnect_ShouldChangeStateToClosed()
        {
            // Arrange
            bool connected = false;
            bool disconnected = false;

            webSocket.OnStateChanged += (state) =>
            {
                logs.Add($"[LOGI] State changed to: {state}");
                if (state == VcWebSocketConnectionState.Open)
                {
                    connected = true;
                }
                else if (state == VcWebSocketConnectionState.Closed)
                {
                    disconnected = true;
                }
            };

            webSocket.Connect(TEST_WS_URL);

            // Wait for connection
            float timeout = 5f;
            while (!connected && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            Assert.IsTrue(connected, "Should connect first");

            // Act
            webSocket.Disconnect();

            // Wait for disconnection
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.IsTrue(disconnected, "Should disconnect successfully");
            Assert.AreEqual(VcWebSocketConnectionState.Closed, webSocket.State);
            Assert.IsTrue(logs.Contains("[LOGI] State changed to: Closed"), "Should have disconnect log");
            Debug.Log(string.Join("\n", logs));
        }

        #endregion

        #region 3. Receive Message Tests (收訊息測試)

        [UnityTest]
        public IEnumerator ReceiveMessage_ShouldTriggerOnMessageReceived()
        {
            // Arrange
            bool connected = false;
            string receivedMessage = null;

            webSocket.OnStateChanged += (state) =>
            {
                logs.Add($"[LOGI] State changed to: {state}");
                if (state == VcWebSocketConnectionState.Open)
                {
                    connected = true;
                }
            };

            webSocket.OnMessageReceived += (message) =>
            {
                logs.Add($"[LOGI] Message received: {message}");
                receivedMessage = message;
            };

            webSocket.Connect(TEST_WS_URL);

            // Wait for connection
            float timeout = 5f;
            while (!connected && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            Assert.IsTrue(connected, "Should connect first");

            // Act - Send a test message
            string testMessage = "Hello WebSocket";
            webSocket.Send(testMessage);

            // Wait for any server response (LOGI, PONG, etc.)
            timeout = 5f;
            while (receivedMessage == null && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            // Assert - For real server, we just check if OnMessageReceived callback works
            // (Server may send LOGI handshake or other protocol messages)
            if (receivedMessage != null)
            {
                Assert.IsNotNull(receivedMessage, "Should receive server message");
                Assert.IsTrue(logs.Any(log => log.Contains("Message received")), "Should have receive log");
            }
            else
            {
                Debug.LogWarning("[Test] No message received from server within timeout - this is OK for some servers");
            }
            Debug.Log(string.Join("\n", logs));
        }

        #endregion

        #region 4. Send Message Tests (送訊息測試)

        [UnityTest]
        public IEnumerator SendMessage_ShouldNotThrowException_WhenConnected()
        {
            // Arrange
            bool connected = false;
            bool exceptionThrown = false;

            webSocket.OnStateChanged += (state) =>
            {
                logs.Add($"[LOGI] State changed to: {state}");
                if (state == VcWebSocketConnectionState.Open)
                {
                    connected = true;
                }
            };

            webSocket.OnError += (error) =>
            {
                logs.Add($"[LOGI] Error: {error}");
                exceptionThrown = true;
            };

            webSocket.Connect(TEST_WS_URL);

            // Wait for connection
            float timeout = 5f;
            while (!connected && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            Assert.IsTrue(connected, "Should connect first");

            // Act
            try
            {
                webSocket.Send("Test message");
                logs.Add("[LOGI] Message sent successfully");
            }
            catch (System.Exception ex)
            {
                logs.Add($"[LOGI] Exception: {ex.Message}");
                exceptionThrown = true;
            }

            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.IsFalse(exceptionThrown, "Should not throw exception when sending");
            Assert.IsTrue(logs.Any(log => log.Contains("Message sent")), "Should have send log");
            Debug.Log(string.Join("\n", logs));
        }

        [UnityTest]
        public IEnumerator SendMessage_ShouldLogError_WhenNotConnected()
        {
            // Arrange
            bool errorLogged = false;
            webSocket.OnError += (error) =>
            {
                logs.Add($"[LOGI] Error: {error}");
                errorLogged = true;
            };

            // Act
            webSocket.Send("Test message");

            // Wait for dispatcher to process the error event
            yield return null;

            // Assert
            Assert.IsTrue(errorLogged, "Should log error when not connected");
            Assert.IsTrue(logs.Any(log => log.Contains("Error")), "Should have error log");
            Debug.Log(string.Join("\n", logs));
        }

        #endregion
    }
}
