// -----------------------------------------------------------------------------
//
// SDK Integration Tests - Group Channel & Messaging
//
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using VyinChatSdk;

namespace VyinChatSdk.Tests
{
    /// <summary>
    /// Integration tests for the full SDK workflow:
    /// 1. Initialize SDK
    /// 2. Connect user
    /// 3. Create group channel
    /// 4. Send message
    /// </summary>
    public class VcSdkIntegrationTests
    {
        // PROD environment configuration
        private const string PROD_APP_ID = "adb53e88-4c35-469a-a888-9e49ef1641b2";
        private const string PROD_DOMAIN = "gamania.chat";
        private const string TEST_USER_ID = "tester01";
        private const string TEST_USER_ID_2 = "tester02";

        private bool isInitialized = false;
        private bool isConnected = false;
        private string currentChannelUrl = null;
        private List<string> logs;

        [SetUp]
        public void SetUp()
        {
            logs = new List<string>();
            isInitialized = false;
            isConnected = false;
            currentChannelUrl = null;

            Log($"[Test Setup] Environment: PROD");
            Log($"[Test Setup] App ID: {PROD_APP_ID}");
            Log($"[Test Setup] Domain: {PROD_DOMAIN}");
        }

        [TearDown]
        public void TearDown()
        {
            logs.Clear();
        }

        #region 1. SDK Initialization & Connection Tests

        [UnityTest]
        public IEnumerator Init_ShouldSucceed()
        {
            // Arrange & Act
            VyinChat.SetConfiguration(PROD_APP_ID, PROD_DOMAIN);
            VyinChat.Init(new VcInitParams(PROD_APP_ID));
            isInitialized = true;

            // Wait a bit for initialization
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.IsTrue(isInitialized, "SDK should be initialized");
            Log("[Test] SDK initialized successfully");
            Debug.Log(string.Join("\n", logs));
        }

        [UnityTest]
        public IEnumerator Connect_ShouldSucceed()
        {
            // Arrange
            VyinChat.SetConfiguration(PROD_APP_ID, PROD_DOMAIN);
            VyinChat.Init(new VcInitParams(PROD_APP_ID));

            bool connectionComplete = false;
            string connectionError = null;

            // Act
            VyinChat.Connect(TEST_USER_ID, null, (user, error) =>
            {
                connectionComplete = true;
                connectionError = error;

                if (string.IsNullOrEmpty(error))
                {
                    isConnected = true;
                    Log($"[Test] Connected as user: {user.UserId}");
                }
                else
                {
                    Log($"[Test] Connection failed: {error}");
                }
            });

            // Wait for connection (max 10 seconds)
            float timeout = 10f;
            while (!connectionComplete && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            // Assert
            Assert.IsTrue(connectionComplete, "Connection callback should be called");
            Assert.IsNull(connectionError, $"Connection should succeed without error. Error: {connectionError}");
            Assert.IsTrue(isConnected, "Should be connected");
            Debug.Log(string.Join("\n", logs));
        }

        #endregion

        #region 2. Group Channel Creation Tests

        [UnityTest]
        public IEnumerator CreateGroupChannel_ShouldSucceed()
        {
            // Arrange - Initialize and Connect
            yield return ConnectUser();
            Assert.IsTrue(isConnected, "Must be connected before creating channel");

            bool channelCreated = false;
            string channelError = null;
            VcGroupChannel createdChannel = null;

            // Prepare channel creation parameters
            var channelParams = new VcGroupChannelCreateParams
            {
                Name = "Unity Test Channel",
                UserIds = new List<string> { TEST_USER_ID, TEST_USER_ID_2 },
                OperatorUserIds = new List<string> { TEST_USER_ID },
                IsDistinct = true
            };

            Log($"[Test] Creating channel with users: {string.Join(", ", channelParams.UserIds)}");

            // Act
            VcGroupChannelModule.CreateGroupChannel(channelParams, (channel, error) =>
            {
                channelCreated = true;
                channelError = error;
                createdChannel = channel;

                if (string.IsNullOrEmpty(error))
                {
                    currentChannelUrl = channel.ChannelUrl;
                    Log($"[Test] Channel created: {channel.Name}");
                    Log($"[Test] Channel URL: {channel.ChannelUrl}");
                }
                else
                {
                    Log($"[Test] Channel creation failed: {error}");
                }
            });

            // Wait for channel creation (max 10 seconds)
            float timeout = 10f;
            while (!channelCreated && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            // Assert
            Assert.IsTrue(channelCreated, "Channel creation callback should be called");
            Assert.IsNull(channelError, $"Channel creation should succeed. Error: {channelError}");
            Assert.IsNotNull(createdChannel, "Created channel should not be null");
            Assert.IsNotNull(currentChannelUrl, "Channel URL should be set");
            Assert.IsFalse(string.IsNullOrEmpty(currentChannelUrl), "Channel URL should not be empty");

            Debug.Log(string.Join("\n", logs));
        }

        #endregion

        #region 3. Send Message Tests

        [UnityTest]
        public IEnumerator SendMessage_ShouldSucceed()
        {
            // Arrange - Connect and Create Channel
            yield return ConnectUser();
            yield return CreateChannel();

            Assert.IsTrue(isConnected, "Must be connected");
            Assert.IsNotNull(currentChannelUrl, "Must have a channel");

            bool messageSent = false;
            string messageError = null;
            string testMessage = $"Hello from Unity Test! Time: {System.DateTime.Now:HH:mm:ss}";

            Log($"[Test] Sending message to channel: {currentChannelUrl}");
            Log($"[Test] Message: {testMessage}");

            // Act
            VyinChat.SendMessage(currentChannelUrl, testMessage, (result, error) =>
            {
                messageSent = true;
                messageError = error;

                if (string.IsNullOrEmpty(error))
                {
                    Log($"[Test] Message sent successfully");
                    Log($"[Test] Result: {result}");
                }
                else
                {
                    Log($"[Test] Message send failed: {error}");
                }
            });

            // Wait for message to send (max 10 seconds)
            float timeout = 10f;
            while (!messageSent && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            // Assert
            Assert.IsTrue(messageSent, "Message send callback should be called");
            Assert.IsNull(messageError, $"Message should be sent successfully. Error: {messageError}");

            Debug.Log(string.Join("\n", logs));
        }

        #endregion

        #region 4. Full Workflow Test

        [UnityTest]
        public IEnumerator FullWorkflow_InitConnectCreateChannelSendMessage_ShouldSucceed()
        {
            Log("=".PadRight(60, '='));
            Log("[Test] Starting Full Workflow Test");
            Log("=".PadRight(60, '='));

            // Step 1: Initialize SDK
            Log("\n[Step 1] Initializing SDK...");
            VyinChat.SetConfiguration(PROD_APP_ID, PROD_DOMAIN);
            VyinChat.Init(new VcInitParams(PROD_APP_ID));
            yield return new WaitForSeconds(0.5f);
            Log("[Step 1] ✓ SDK Initialized");

            // Step 2: Connect User
            Log("\n[Step 2] Connecting user...");
            yield return ConnectUser();
            Assert.IsTrue(isConnected, "User should be connected");
            Log($"[Step 2] ✓ Connected as: {TEST_USER_ID}");

            // Step 3: Create Group Channel
            Log("\n[Step 3] Creating group channel...");
            yield return CreateChannel();
            Assert.IsNotNull(currentChannelUrl, "Channel should be created");
            Log($"[Step 3] ✓ Channel created: {currentChannelUrl}");

            // Step 4: Send Message
            Log("\n[Step 4] Sending message...");
            bool messageSent = false;
            string messageError = null;
            string testMessage = "Full workflow test message!";

            VyinChat.SendMessage(currentChannelUrl, testMessage, (result, error) =>
            {
                messageSent = true;
                messageError = error;
                if (string.IsNullOrEmpty(error))
                {
                    Log($"[Step 4] ✓ Message sent: {testMessage}");
                }
            });

            float timeout = 10f;
            while (!messageSent && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }

            Assert.IsTrue(messageSent, "Message should be sent");
            Assert.IsNull(messageError, "No error should occur");

            Log("\n" + "=".PadRight(60, '='));
            Log("[Test] ✓ Full Workflow Completed Successfully!");
            Log("=".PadRight(60, '='));
            Debug.Log(string.Join("\n", logs));
        }

        #endregion

        #region Helper Methods

        private IEnumerator ConnectUser()
        {
            bool connectionComplete = false;

            VyinChat.SetConfiguration(PROD_APP_ID, PROD_DOMAIN);
            VyinChat.Init(new VcInitParams(PROD_APP_ID));
            yield return new WaitForSeconds(0.5f);

            VyinChat.Connect(TEST_USER_ID, null, (user, error) =>
            {
                connectionComplete = true;
                if (string.IsNullOrEmpty(error))
                {
                    isConnected = true;
                }
            });

            float timeout = 10f;
            while (!connectionComplete && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }
        }

        private IEnumerator CreateChannel()
        {
            bool channelCreated = false;

            var channelParams = new VcGroupChannelCreateParams
            {
                Name = "Unity Integration Test Channel",
                UserIds = new List<string> { TEST_USER_ID, TEST_USER_ID_2 },
                OperatorUserIds = new List<string> { TEST_USER_ID },
                IsDistinct = true
            };

            VcGroupChannelModule.CreateGroupChannel(channelParams, (channel, error) =>
            {
                channelCreated = true;
                if (string.IsNullOrEmpty(error))
                {
                    currentChannelUrl = channel.ChannelUrl;
                }
            });

            float timeout = 10f;
            while (!channelCreated && timeout > 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }
        }

        private void Log(string message)
        {
            logs.Add(message);
            Debug.Log(message);
        }

        #endregion
    }
}
