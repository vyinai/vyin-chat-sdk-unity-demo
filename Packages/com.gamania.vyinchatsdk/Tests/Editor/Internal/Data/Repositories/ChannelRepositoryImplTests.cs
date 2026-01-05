// Tests/Editor/Internal/Data/Repositories/ChannelRepositoryImplTests.cs
// Unit tests for ChannelRepositoryImpl
// Phase 3: Task 3.3 - GetChannel tests

using NUnit.Framework;
using System;
using System.Collections.Generic;
using VyinChatSdk.Internal.Data.Network;
using VyinChatSdk.Internal.Data.Repositories;
using VyinChatSdk.Tests.Mocks.Platform;

namespace VyinChatSdk.Tests.Editor.Internal.Data.Repositories
{
    [TestFixture]
    public class ChannelRepositoryImplTests
    {
        private MockHttpClient _mockHttpClient;
        private ChannelRepositoryImpl _repository;
        private const string TestChannelUrl = "test_channel_url";
        private const string TestSessionKey = "test-session-key-12345";
        private const string BaseApiUrl = "https://api.gamania.chat";

        [SetUp]
        public void SetUp()
        {
            _mockHttpClient = new MockHttpClient();
            _repository = new ChannelRepositoryImpl(_mockHttpClient, BaseApiUrl);
        }

        [TearDown]
        public void TearDown()
        {
            _mockHttpClient.Reset();
        }

        #region GetChannel - Success Cases

        [Test]
        public void GetChannel_ShouldReturnChannel_WhenSuccess()
        {
            // Arrange
            var expectedJson = "{\"channel_url\":\"test_channel_url\",\"name\":\"Test Channel\"}";
            _mockHttpClient.QueueResponse(new HttpResponse
            {
                StatusCode = 200,
                Body = expectedJson
            });
            _mockHttpClient.SetSessionKey(TestSessionKey);

            // Act
            var result = _repository.GetChannelAsync(TestChannelUrl).GetAwaiter().GetResult();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("test_channel_url", result.ChannelUrl);
            Assert.AreEqual("Test Channel", result.Name);
            Assert.AreEqual(1, _mockHttpClient.RequestHistory.Count);
        }

        [Test]
        public void GetChannel_ShouldUseSessionKey_InRequest()
        {
            // Arrange
            var expectedJson = "{\"channel_url\":\"test_channel_url\",\"name\":\"Test Channel\"}";
            _mockHttpClient.QueueResponse(new HttpResponse
            {
                StatusCode = 200,
                Body = expectedJson
            });
            _mockHttpClient.SetSessionKey(TestSessionKey);

            // Act
            var result = _repository.GetChannelAsync(TestChannelUrl).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, _mockHttpClient.RequestHistory.Count);
            var request = _mockHttpClient.RequestHistory[0];
            Assert.IsTrue(request.headers.ContainsKey("Session-Key"),
                "Request should include Session-Key header");
            Assert.AreEqual(TestSessionKey, request.headers["Session-Key"],
                "Session-Key header should match the set session key");
        }

        #endregion

        #region GetChannel - Error Cases

        [Test]
        public void GetChannel_ShouldThrow_When404_ChannelNotFound()
        {
            // Arrange
            _mockHttpClient.QueueResponse(new HttpResponse
            {
                StatusCode = 404,
                Body = "{\"error\":\"Channel not found\"}"
            });
            _mockHttpClient.SetSessionKey(TestSessionKey);

            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _repository.GetChannelAsync(TestChannelUrl).GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.ChannelNotFound, ex.ErrorCode);
            Assert.That(ex.Message, Does.Contain("not found").IgnoreCase);
        }

        [Test]
        public void GetChannel_ShouldThrow_When403_InvalidSessionKey()
        {
            // Arrange
            _mockHttpClient.QueueResponse(new HttpResponse
            {
                StatusCode = 403,
                Body = "{\"error\":\"Invalid session key\"}"
            });
            _mockHttpClient.SetSessionKey("invalid-session-key");

            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _repository.GetChannelAsync(TestChannelUrl).GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.InvalidSessionKey, ex.ErrorCode);
            Assert.That(ex.Message, Does.Contain("session key").IgnoreCase);
        }

        [Test]
        public void GetChannel_BeforeConnect_ShouldThrow_NoSessionKey()
        {
            // Arrange - Don't set session key (simulating before WebSocket connect)
            _mockHttpClient.QueueResponse(new HttpResponse
            {
                StatusCode = 401,
                Body = "{\"error\":\"No session key\"}"
            });

            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _repository.GetChannelAsync(TestChannelUrl).GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.InvalidSessionKey, ex.ErrorCode);
            Assert.That(ex.Message, Does.Contain("session key").IgnoreCase);
        }

        [Test]
        public void GetChannel_ShouldThrow_When500_ServerError()
        {
            // Arrange
            _mockHttpClient.QueueResponse(new HttpResponse
            {
                StatusCode = 500,
                Body = "{\"error\":\"Internal server error\"}"
            });
            _mockHttpClient.SetSessionKey(TestSessionKey);

            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _repository.GetChannelAsync(TestChannelUrl).GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.InternalServerError, ex.ErrorCode);
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void Constructor_NullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ChannelRepositoryImpl(null, BaseApiUrl));
        }

        [Test]
        public void Constructor_NullBaseUrl_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ChannelRepositoryImpl(_mockHttpClient, null));
        }

        #endregion
    }
}
