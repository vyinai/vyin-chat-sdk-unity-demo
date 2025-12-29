// -----------------------------------------------------------------------------
//
// WebSocket Transport Tests (TDD - Phase 1)
//
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using VyinChatSdk.Transport;
using VyinChatSdk.Transport.Protocol;
using VyinChatSdk.WebSocket;

namespace VyinChatSdk.Tests.Transport
{
    /// <summary>
    /// Phase 1 Tests: WebSocket Connection and LOGI Authentication
    /// </summary>
    public class WebSocketTransportTests
    {
        private const string TEST_APP_ID = "adb53e88-4c35-469a-a888-9e49ef1641b2";
        private const string TEST_DOMAIN = "gamania.chat";
        private const string TEST_USER_ID = "testuser1";
        private const string TEST_ACCESS_TOKEN = "test_token_123";

        private MockVcWebSocket mockWebSocket;
        private WebSocketTransport transport;
        private List<string> logs;

        [SetUp]
        public void SetUp()
        {
            logs = new List<string>();
            mockWebSocket = new MockVcWebSocket();
            transport = new WebSocketTransport(TEST_APP_ID, TEST_DOMAIN, mockWebSocket);
        }

        [TearDown]
        public void TearDown()
        {
            transport?.Disconnect();
            transport = null;
            mockWebSocket = null;
            logs.Clear();
        }

        #region Task 1.1 & 1.2: WebSocket Connection Tests

        [Test]
        public void Connect_ShouldUseWSS_WithQueryParams()
        {
            // Act
            transport.Connect(TEST_USER_ID, TEST_ACCESS_TOKEN);

            // Assert
            string capturedUrl = mockWebSocket.LastConnectedUrl;
            Assert.IsNotNull(capturedUrl, "WebSocket Connect should be called");
            Assert.IsTrue(capturedUrl.StartsWith("wss://"), "Should use WSS protocol");
            Assert.IsTrue(capturedUrl.Contains(TEST_APP_ID), "Should include app ID in URL");
            Assert.IsTrue(capturedUrl.Contains(TEST_DOMAIN), "Should include domain in URL");

            Debug.Log($"[TEST] Captured URL: {capturedUrl}");
        }

        [Test]
        public void Connect_ShouldInclude_UserId_AccessToken()
        {
            // Act
            transport.Connect(TEST_USER_ID, TEST_ACCESS_TOKEN);

            // Assert
            string capturedUrl = mockWebSocket.LastConnectedUrl;
            Assert.IsNotNull(capturedUrl, "WebSocket Connect should be called");
            Assert.IsTrue(capturedUrl.Contains("user_id="), "Should include user_id parameter");
            Assert.IsTrue(capturedUrl.Contains(TEST_USER_ID), "Should include actual user ID");
            // Note: access_token might be URL encoded

            Debug.Log($"[TEST] URL contains user_id: {capturedUrl}");
        }

        [Test]
        public void Connect_ShouldTriggerOnConnected_AfterHandshake()
        {
            // Arrange
            TransportConnectionState? connectedState = null;
            transport.OnStateChanged += (state) =>
            {
                if (state == TransportConnectionState.Connected)
                    connectedState = state;
            };

            // Act
            transport.Connect(TEST_USER_ID, TEST_ACCESS_TOKEN);

            // Simulate WebSocket connected
            mockWebSocket.SimulateConnected();

            // Assert
            Assert.AreEqual(TransportConnectionState.Connected, connectedState);
            Assert.AreEqual(TransportConnectionState.Connected, transport.State);
        }

        [Test]
        public void Connect_ShouldFail_WithInvalidToken()
        {
            // This test will be implemented when we have real server validation
            // For now, we test that the transport accepts any token and sends it

            // Arrange
            string invalidToken = "";

            // Act
            transport.Connect(TEST_USER_ID, invalidToken);

            // Assert
            Assert.IsNotNull(mockWebSocket.LastConnectedUrl);
            // Server will reject invalid token in LOGI response
        }

        #endregion

        #region Task 1.3 & 1.4: LOGI Command Handling Tests

        [Test]
        public void OnConnected_ShouldSendLOGI()
        {
            // Act
            transport.Connect(TEST_USER_ID, TEST_ACCESS_TOKEN);
            mockWebSocket.SimulateConnected();

            // Assert
            Assert.AreEqual(1, mockWebSocket.SentMessages.Count, "Should send one message");
            string capturedCommand = mockWebSocket.SentMessages[0];
            Assert.IsTrue(capturedCommand.StartsWith("LOGI"), "Should send LOGI command");
            Assert.IsTrue(capturedCommand.Contains(TEST_USER_ID), "LOGI should contain user_id");

            Debug.Log($"[TEST] LOGI command: {capturedCommand}");
        }

        [Test]
        public void ReceiveLOGI_ShouldExtractSessionKey()
        {
            // Arrange
            string testSessionKey = "test_session_key_12345";
            string logiResponse = $"LOGI{{\"key\":\"{testSessionKey}\",\"req_id\":\"123\"}}";

            string capturedSessionKey = null;
            transport.OnAuthenticated += (sessionKey) =>
            {
                capturedSessionKey = sessionKey;
            };

            // Act
            transport.Connect(TEST_USER_ID, TEST_ACCESS_TOKEN);
            mockWebSocket.SimulateConnected();
            mockWebSocket.SimulateMessage(logiResponse);

            // Assert
            Assert.AreEqual(testSessionKey, capturedSessionKey);
            Assert.AreEqual(testSessionKey, transport.SessionKey);

            Debug.Log($"[TEST] Session key extracted: {testSessionKey}");
        }

        [Test]
        public void ReceiveLOGI_ShouldStoreSessionKey()
        {
            // Arrange
            string testSessionKey = "stored_session_key";
            string logiResponse = $"LOGI{{\"key\":\"{testSessionKey}\"}}";

            // Act
            transport.Connect(TEST_USER_ID, TEST_ACCESS_TOKEN);
            mockWebSocket.SimulateConnected();
            mockWebSocket.SimulateMessage(logiResponse);

            // Assert
            Assert.AreEqual(testSessionKey, transport.SessionKey);
            Assert.AreEqual(TransportConnectionState.Authenticated, transport.State);
        }

        [UnityTest]
        public IEnumerator NoLOGI_Within10Seconds_ShouldTimeout()
        {
            // Arrange
            string errorMessage = null;
            transport.OnAuthenticationFailed += (error) =>
            {
                errorMessage = error;
            };

            // Act
            transport.Connect(TEST_USER_ID, TEST_ACCESS_TOKEN);
            mockWebSocket.SimulateConnected();

            // Simulate 10+ seconds passing
            float elapsed = 0f;
            while (elapsed < 10.5f)
            {
                transport.Update(); // Call update to check timeout
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;
            }

            // Assert
            Assert.IsNotNull(errorMessage, "Should trigger authentication failed");
            Assert.IsTrue(errorMessage.Contains("timeout"), "Error should mention timeout");
            Assert.AreEqual(TransportConnectionState.Closed, transport.State);

            Debug.Log($"[TEST] Timeout error: {errorMessage}");
        }

        [UnityTest]
        public IEnumerator InvalidLOGI_ShouldTriggerAuthFailed()
        {
            // Arrange
            string errorMessage = null;
            transport.OnAuthenticationFailed += (error) =>
            {
                errorMessage = error;
            };

            // Act
            transport.Connect(TEST_USER_ID, TEST_ACCESS_TOKEN);
            mockWebSocket.SimulateConnected();

            // Send invalid LOGI response (with error)
            mockWebSocket.SimulateMessage("LOGI{\"error\":\"Invalid credentials\"}");

            // Wait one frame to ensure all events are processed
            yield return null;

            // Assert
            Assert.IsNotNull(errorMessage, "Should trigger authentication failed");
            Assert.IsTrue(errorMessage.Contains("Invalid credentials"), "Should contain error message");
            Assert.AreEqual(TransportConnectionState.Closed, transport.State);
        }

        #endregion

        #region Task 1.5: Command Protocol Tests

        [Test]
        public void SendCommand_ShouldGenerateUniqueReqId()
        {
            // Arrange
            RequestIdGenerator.Reset();

            // Act
            string reqId1 = RequestIdGenerator.Generate();
            string reqId2 = RequestIdGenerator.Generate();
            string reqId3 = RequestIdGenerator.Generate();

            // Assert
            Assert.AreNotEqual(reqId1, reqId2, "Request IDs should be unique");
            Assert.AreNotEqual(reqId2, reqId3, "Request IDs should be unique");
            Assert.AreNotEqual(reqId1, reqId3, "Request IDs should be unique");

            Debug.Log($"[TEST] Generated req_ids: {reqId1}, {reqId2}, {reqId3}");
        }

        [Test]
        public void SendCommand_ShouldSerialize_Correctly()
        {
            // Arrange
            var logiCommand = new LogiCommand
            {
                UserId = "user123",
                AccessToken = "token456",
                ReqId = "req789"
            };

            // Act
            string serialized = logiCommand.Serialize();

            // Assert
            Assert.IsTrue(serialized.StartsWith("LOGI"), "Should start with command type");
            Assert.IsTrue(serialized.Contains("user123"), "Should contain user ID");
            Assert.IsTrue(serialized.Contains("token456"), "Should contain access token");
            Assert.IsTrue(serialized.Contains("req789"), "Should contain request ID");

            Debug.Log($"[TEST] Serialized command: {serialized}");
        }

        #endregion
    }
}
