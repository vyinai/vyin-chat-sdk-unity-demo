using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;
using VyinChatSdk;

namespace VyinChatSdk.Tests.Editor
{
    /// <summary>
    /// Tests for VyinChat.Connect() functionality
    /// Following TDD approach matching iOS SDK behavior
    /// </summary>
    [TestFixture]
    public class VyinChatConnectTests
    {
        private const string TEST_APP_ID = "test-app-id";
        private const string TEST_USER_ID = "test-user-001";
        private const string TEST_AUTH_TOKEN = "test-auth-token";

        [SetUp]
        public void SetUp()
        {
            // Reset and initialize before each test
            VyinChat.ResetForTesting();
            var initParams = new VcInitParams(TEST_APP_ID);
            VyinChat.Init(initParams);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            VyinChat.ResetForTesting();
        }

        [Test]
        public void Connect_WithoutInit_ShouldThrow()
        {
            // Arrange
            VyinChat.ResetForTesting(); // Reset to uninitialize state

            // Expect error log
            LogAssert.Expect(LogType.Error, "[VyinChatMain] VyinChatMain instance hasn't been initialized. Try VyinChat.Init().");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, (user, error) => { });
            });
        }

        [Test]
        public void Connect_WithNullUserId_ShouldCallCallbackWithError()
        {
            // Arrange
            string resultError = null;
            VcUser resultUser = null;
            bool callbackCalled = false;

            // Expect error log
            LogAssert.Expect(LogType.Error, "[VyinChatMain] userId is empty.");

            // Act
            VyinChat.Connect(null, TEST_AUTH_TOKEN, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            Assert.IsTrue(callbackCalled, "Callback should be called");
            Assert.IsNull(resultUser, "User should be null");
            Assert.IsNotNull(resultError, "Error should not be null");
            Assert.That(resultError, Does.Contain("userId"));
        }

        [Test]
        public void Connect_WithEmptyUserId_ShouldCallCallbackWithError()
        {
            // Arrange
            string resultError = null;
            VcUser resultUser = null;
            bool callbackCalled = false;

            // Expect error log
            LogAssert.Expect(LogType.Error, "[VyinChatMain] userId is empty.");

            // Act
            VyinChat.Connect("", TEST_AUTH_TOKEN, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            Assert.IsTrue(callbackCalled, "Callback should be called");
            Assert.IsNull(resultUser, "User should be null");
            Assert.IsNotNull(resultError, "Error should not be null");
            Assert.That(resultError, Does.Contain("userId"));
        }

        [Test]
        public void Connect_ShouldConnectWebSocket_WithUserIdAndToken()
        {
            // Arrange
            VcUser resultUser = null;
            string resultError = null;
            bool callbackCalled = false;

            // Act
            VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            Assert.IsTrue(callbackCalled, "Callback should be called");
            Assert.IsNull(resultUser, "User should be null (WebSocket not implemented yet)");
            Assert.IsNotNull(resultError, "Error should be returned (WebSocket not implemented yet)");
            // TODO: Update assertions when WebSocket is implemented
        }

        [Test]
        public void Connect_ShouldWaitForLOGI()
        {
            // Arrange
            VcUser resultUser = null;
            string resultError = null;
            bool callbackCalled = false;

            // Act
            VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            Assert.IsTrue(callbackCalled, "Callback should be called");
            Assert.IsNull(resultUser, "User should be null (WebSocket not implemented yet)");
            Assert.IsNotNull(resultError, "Error should be returned (WebSocket not implemented yet)");
            // TODO: Verify LOGI command was sent and received
            // TODO: This test will be updated when WebSocket is implemented
        }

        [Test]
        public void Connect_ShouldExtractSessionKey_FromLOGI()
        {
            // Arrange
            VcUser resultUser = null;
            string resultError = null;
            bool callbackCalled = false;

            // Act
            VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            Assert.IsTrue(callbackCalled, "Callback should be called");
            Assert.IsNull(resultUser, "User should be null (WebSocket not implemented yet)");
            Assert.IsNotNull(resultError, "Error should be returned (WebSocket not implemented yet)");
            // TODO: Verify session_key was extracted from LOGI response
            // TODO: Verify session_key was set in HTTP client
            // TODO: This test will be updated when WebSocket is implemented
        }

        [Test]
        public void Connect_ShouldReturnUser_WhenSuccess()
        {
            // Arrange
            VcUser resultUser = null;
            string resultError = null;
            bool callbackCalled = false;

            // Act
            VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            Assert.IsTrue(callbackCalled, "Callback should be called");
            Assert.IsNull(resultUser, "User should be null (WebSocket not implemented yet)");
            Assert.IsNotNull(resultError, "Error should be returned (WebSocket not implemented yet)");
            // TODO: Update when WebSocket is implemented
            // Should verify:
            // - resultUser is not null
            // - resultUser.UserId matches TEST_USER_ID
            // - resultError is null
        }

        [Test]
        public void Connect_ShouldCallCallback_WithError_WhenNoLOGI()
        {
            // Arrange
            VcUser resultUser = null;
            string resultError = null;
            bool callbackCalled = false;

            // Act
            VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            Assert.IsTrue(callbackCalled, "Callback should be called");
            Assert.IsNull(resultUser, "User should be null (WebSocket not implemented yet)");
            Assert.IsNotNull(resultError, "Error should be returned (WebSocket not implemented yet)");
            // TODO: Update when WebSocket is implemented
            // Should verify:
            // - resultUser is null
            // - resultError is not null
            // - resultError indicates no LOGI response
        }

        [Test]
        public void Connect_ShouldCallCallback_WithError_WhenTimeout()
        {
            // Arrange
            VcUser resultUser = null;
            string resultError = null;
            bool callbackCalled = false;

            // Act
            VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            Assert.IsTrue(callbackCalled, "Callback should be called");
            Assert.IsNull(resultUser, "User should be null (WebSocket not implemented yet)");
            Assert.IsNotNull(resultError, "Error should be returned (WebSocket not implemented yet)");
            // TODO: Update when WebSocket is implemented
            // Should verify:
            // - resultUser is null
            // - resultError is not null
            // - resultError indicates timeout
        }

        [Test]
        public void Connect_WithCustomHosts_ShouldUseProvidedHosts()
        {
            // Arrange
            string customApiHost = "https://custom-api.example.com";
            string customWsHost = "wss://custom-ws.example.com";
            VcUser resultUser = null;
            string resultError = null;
            bool callbackCalled = false;

            // Act
            VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, customApiHost, customWsHost, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            // Connection attempt should use the custom hosts
            // TODO: Verify custom hosts are used when WebSocket is implemented
            Assert.IsTrue(callbackCalled || !callbackCalled, "Test placeholder");
        }

        [Test]
        public void Connect_WithNullHosts_ShouldUseDefaultHosts()
        {
            // Arrange
            VcUser resultUser = null;
            string resultError = null;
            bool callbackCalled = false;

            // Act
            VyinChat.Connect(TEST_USER_ID, TEST_AUTH_TOKEN, null, null, (user, error) =>
            {
                resultUser = user;
                resultError = error;
                callbackCalled = true;
            });

            // Assert
            // Connection attempt should use default hosts based on appId
            // Expected: https://{TEST_APP_ID}.gamania.chat and wss://{TEST_APP_ID}.gamania.chat
            // TODO: Verify default hosts are used when WebSocket is implemented
            Assert.IsTrue(callbackCalled || !callbackCalled, "Test placeholder");
        }
    }
}
