// Tests/Runtime/Domain/GetChannelUseCaseTests.cs
// Unit tests for GetChannelUseCase
// Phase 3: Task 3.1 - Test preparation

using NUnit.Framework;
using System;
using VyinChatSdk.Internal.Domain.UseCases;
using VyinChatSdk.Tests.Mocks.Data;

namespace VyinChatSdk.Tests.Internal.Domain.UseCases
{
    [TestFixture]
    public class GetChannelUseCaseTests
    {
        private MockChannelRepository _mockRepository;
        private GetChannelUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockChannelRepository();
            _useCase = new GetChannelUseCase(_mockRepository);
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.Reset();
        }

        #region Success Cases

        [Test]
        public void ExecuteAsync_ValidChannelUrl_ReturnsChannel()
        {
            // Arrange
            var expectedChannel = new VcGroupChannel
            {
                ChannelUrl = "test_channel_url",
                Name = "Test Channel"
            };
            _mockRepository.AddChannel(expectedChannel);

            // Act
            var result = _useCase.ExecuteAsync("test_channel_url").GetAwaiter().GetResult();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedChannel.ChannelUrl, result.ChannelUrl);
            Assert.AreEqual(expectedChannel.Name, result.Name);
            Assert.AreEqual(1, _mockRepository.OperationHistory.Count);
            Assert.AreEqual("GetChannel", _mockRepository.OperationHistory[0].operation);
        }

        #endregion

        #region Validation Error Cases

        [Test]
        public void ExecuteAsync_NullChannelUrl_ThrowsVcException()
        {
            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _useCase.ExecuteAsync(null).GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.InvalidParameter, ex.ErrorCode);
            Assert.That(ex.Message, Does.Contain("Channel URL"));
        }

        [Test]
        public void ExecuteAsync_EmptyChannelUrl_ThrowsVcException()
        {
            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _useCase.ExecuteAsync("").GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.InvalidParameter, ex.ErrorCode);
        }

        [Test]
        public void ExecuteAsync_WhitespaceChannelUrl_ThrowsVcException()
        {
            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _useCase.ExecuteAsync("   ").GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.InvalidParameter, ex.ErrorCode);
        }

        #endregion

        #region Not Found Cases

        [Test]
        public void ExecuteAsync_ChannelNotFound_ThrowsVcException()
        {
            // Arrange - Don't add any channel to repository

            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _useCase.ExecuteAsync("non_existent_channel").GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.ChannelNotFound, ex.ErrorCode);
            Assert.That(ex.Message, Does.Contain("Channel not found"));
        }

        #endregion

        #region Error Handling Cases

        [Test]
        public void ExecuteAsync_RepositoryThrowsException_WrapsInVcException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Repository error");
            _mockRepository.SetExceptionToThrow(innerException);

            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _useCase.ExecuteAsync("test_channel").GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.Unknown, ex.ErrorCode);
            Assert.IsNotNull(ex.InnerException);
            Assert.AreEqual(innerException, ex.InnerException);
        }

        [Test]
        public void ExecuteAsync_RepositoryThrowsVcException_PreservesException()
        {
            // Arrange
            var vcException = new VcException(VcErrorCode.NetworkError, "Network error");
            _mockRepository.SetExceptionToThrow(vcException);

            // Act & Assert
            var ex = Assert.Throws<VcException>(() =>
            {
                _useCase.ExecuteAsync("test_channel").GetAwaiter().GetResult();
            });

            Assert.AreEqual(VcErrorCode.NetworkError, ex.ErrorCode);
            Assert.AreEqual(vcException, ex);
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new GetChannelUseCase(null));
        }

        #endregion
    }
}
