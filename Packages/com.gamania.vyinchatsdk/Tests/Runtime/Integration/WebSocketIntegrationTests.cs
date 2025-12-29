// -----------------------------------------------------------------------------
//
// WebSocket Integration Tests - Real Connection Tests
// Tests using REAL WebSocket connection to test server
//
// Test Server: wss://adb53e88-4c35-469a-a888-9e49ef1641b2.gamania.chat/ws
// Test User: tester01
//
// -----------------------------------------------------------------------------

using NUnit.Framework;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;
using System.Collections;
using Gamania.VyinChatSDK.Data.Network;
using Gamania.VyinChatSDK.Domain.Commands;
using UnityEngine;

namespace Gamania.VyinChatSDK.Tests.Integration
{
    /// <summary>
    /// Integration tests using REAL WebSocket connection
    /// Tests connection, LOGI handling, and session_key extraction
    /// </summary>
    public class WebSocketIntegrationTests
    {
        private UnityWebSocketClient client;
        private WebSocketConfig testConfig;

        // Test environment configuration
        private const string TEST_APP_ID = "adb53e88-4c35-469a-a888-9e49ef1641b2";
        private const string TEST_USER_ID = "tester01";
        private const float CONNECTION_TIMEOUT = 10f;
        private const float LOGI_TIMEOUT = 10f;
        private const string VALID_ENV = "gamania.chat";

        [SetUp]
        public void SetUp()
        {
            client = new UnityWebSocketClient();
            testConfig = new WebSocketConfig
            {
                ApplicationId = TEST_APP_ID,
                UserId = TEST_USER_ID,
                AccessToken = null,  // Token is optional for this test server
                EnvironmentDomain = VALID_ENV
            };
        }

        [TearDown]
        public void TearDown()
        {
            client?.Disconnect();
        }

        /// <summary>
        /// Should use wss and include query params
        /// </summary>
        [Test]
        public void Connect_ShouldUseWSS_WithQueryParams()
        {
            var url = testConfig.BuildWebSocketUrl();

            Assert.IsTrue(url.StartsWith("wss://"), "Should use wss");
            Assert.IsTrue(url.Contains("user_id="), "Should include user_id");
            Assert.IsTrue(url.Contains("ai="), "Should include application id");
        }

        /// <summary>
        /// Should include user_id and access_token in URL (not asserting validity)
        /// </summary>
        [Test]
        public void Connect_ShouldInclude_UserId_AccessToken()
        {
            var cfg = new WebSocketConfig
            {
                ApplicationId = TEST_APP_ID,
                UserId = TEST_USER_ID,
                AccessToken = "valid-token-placeholder",
                EnvironmentDomain = VALID_ENV
            };

            var url = cfg.BuildWebSocketUrl();

            Assert.IsTrue(url.Contains("user_id=" + TEST_USER_ID));
            Assert.IsTrue(url.Contains("access_token=valid-token-placeholder"));
        }

        /// <summary>
        /// OnConnected should fire after handshake
        /// </summary>
        [UnityTest]
        public IEnumerator Connect_ShouldTriggerOnConnected_AfterHandshake()
        {
            bool connected = false;
            string errorMessage = null;

            client.OnConnected += () => connected = true;
            client.OnError += (error) => errorMessage = error;

            client.Connect(testConfig);

            float elapsed = 0f;
            while (!connected && errorMessage == null && elapsed < CONNECTION_TIMEOUT)
            {
                client.Update();
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(connected, $"Should connect to WS. Error: {errorMessage}");
            Assert.IsTrue(client.IsConnected, "IsConnected should be true");
        }

        /// <summary>
        /// After connected, should wait for LOGI
        /// </summary>
        [UnityTest]
        public IEnumerator OnConnected_ShouldWaitForLOGI()
        {
            bool connected = false;
            LogiCommand logiResponse = null;

            client.OnConnected += () => connected = true;
            client.OnMessageReceived += (message) => logiResponse = CommandParser.ParseLogiCommand(message);

            client.Connect(testConfig);

            float elapsed = 0f;
            while (logiResponse == null && elapsed < LOGI_TIMEOUT)
            {
                client.Update();
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(connected, "Should connect first");
            Assert.IsNotNull(logiResponse, "Should receive LOGI");
        }

        /// <summary>
        /// LOGI should carry session_key (record locally in test)
        /// </summary>
        [UnityTest]
        public IEnumerator ReceiveLOGI_ShouldStoreSessionKey()
        {
            string sessionKey = null;

            client.OnMessageReceived += (message) =>
            {
                var logi = CommandParser.ParseLogiCommand(message);
                sessionKey = logi?.SessionKey;
            };

            client.Connect(testConfig);

            float elapsed = 0f;
            while (sessionKey == null && elapsed < LOGI_TIMEOUT)
            {
                client.Update();
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsFalse(string.IsNullOrEmpty(sessionKey), "Should get session_key");
        }

        /// <summary>
        /// LOGI should expose session_key (explicit extract check)
        /// </summary>
        [UnityTest]
        public IEnumerator ReceiveLOGI_ShouldExtractSessionKey()
        {
            LogiCommand logiResponse = null;

            client.OnMessageReceived += (message) =>
            {
                logiResponse = CommandParser.ParseLogiCommand(message);
            };

            client.Connect(testConfig);

            float elapsed = 0f;
            while (logiResponse == null && elapsed < LOGI_TIMEOUT)
            {
                client.Update();
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsNotNull(logiResponse, "Should receive LOGI");
            Assert.IsFalse(string.IsNullOrEmpty(logiResponse.SessionKey), "SessionKey should not be empty");
        }

        /// <summary>
        /// LOGI should include ping/pong settings
        /// </summary>
        [UnityTest]
        public IEnumerator ReceiveLOGI_ShouldIncludePingPongSettings()
        {
            LogiCommand logiResponse = null;

            client.OnMessageReceived += (message) =>
            {
                logiResponse = CommandParser.ParseLogiCommand(message);
            };

            client.Connect(testConfig);

            float elapsed = 0f;
            while (logiResponse == null && elapsed < LOGI_TIMEOUT)
            {
                client.Update();
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsNotNull(logiResponse, "Should receive LOGI");
            Assert.Greater(logiResponse.PingInterval, 0);
            Assert.Greater(logiResponse.PongTimeout, 0);
        }

        /// <summary>
        /// Invalid token should fail (LOGI error or OnError)
        /// </summary>
        [UnityTest]
        public IEnumerator Connect_ShouldFail_WithInvalidToken()
        {
            var invalidConfig = new WebSocketConfig
            {
                ApplicationId = TEST_APP_ID,
                UserId = TEST_USER_ID,
                AccessToken = "invalid_token",
                EnvironmentDomain = VALID_ENV
            };

            bool gotError = false;
            LogiCommand logiResponse = null;
            bool erorMessage = false;

            client.OnError += _ => gotError = true;
            client.OnMessageReceived += (message) =>
            {
                if (message.StartsWith("EROR"))
                {
                    erorMessage = true;
                }
                logiResponse = CommandParser.ParseLogiCommand(message);
            };

            client.Connect(invalidConfig);

            float elapsed = 0f;
            while (!gotError && !erorMessage && (logiResponse == null || logiResponse.IsSuccess()) && elapsed < LOGI_TIMEOUT)
            {
                client.Update();
                elapsed += Time.deltaTime;
                yield return null;
            }

            bool authFailed = gotError || erorMessage || (logiResponse != null && !logiResponse.IsSuccess());
            Assert.IsTrue(authFailed, "Invalid token should fail auth");
        }

        /// <summary>
        /// If no LOGI within timeout, treat as failure (simulate with invalid domain)
        /// </summary>
        [UnityTest]
        public IEnumerator NoLOGI_Within10Seconds_ShouldTimeout()
        {
            var badConfig = new WebSocketConfig
            {
                ApplicationId = TEST_APP_ID,
                UserId = TEST_USER_ID,
                AccessToken = null,
                EnvironmentDomain = "invalid.gamania.chat"
            };

            LogiCommand logiResponse = null;
            bool gotError = false;

            client.OnMessageReceived += (message) => logiResponse = CommandParser.ParseLogiCommand(message);
            client.OnError += _ => gotError = true;

            LogAssert.Expect(LogType.Error, new Regex("WebSocket error"));

            client.Connect(badConfig);

            float elapsed = 0f;
            while (logiResponse == null && !gotError && elapsed < LOGI_TIMEOUT)
            {
                client.Update();
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(gotError || logiResponse == null, "Should error or timeout when no LOGI");
        }

        /// <summary>
        /// Invalid LOGI should flag auth failure
        /// </summary>
        [UnityTest]
        public IEnumerator InvalidLOGI_ShouldTriggerAuthFailed()
        {
            LogiCommand logiResponse = null;
            bool gotError = false;
            bool erorMessage = false;

            client.OnError += _ => gotError = true;
            client.OnMessageReceived += (message) =>
            {
                if (message.StartsWith("EROR"))
                {
                    erorMessage = true;
                }
                logiResponse = CommandParser.ParseLogiCommand(message);
            };

            var invalidTokenConfig = new WebSocketConfig
            {
                ApplicationId = TEST_APP_ID,
                UserId = TEST_USER_ID,
                AccessToken = "invalid_token",
                EnvironmentDomain = VALID_ENV
            };

            client.Connect(invalidTokenConfig);

            float elapsed = 0f;
            while (!gotError && (logiResponse == null || logiResponse.IsSuccess()) && elapsed < LOGI_TIMEOUT)
            {
                client.Update();
                elapsed += Time.deltaTime;
                yield return null;
            }

            bool authFailed = gotError || erorMessage || (logiResponse != null && !logiResponse.IsSuccess());
            Assert.IsTrue(authFailed, "Invalid LOGI or error should fail auth");
        }
    }
}
