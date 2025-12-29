// -----------------------------------------------------------------------------
//
// Command Protocol Tests (Unit Tests)
//
// -----------------------------------------------------------------------------

using NUnit.Framework;
using UnityEngine;
using VyinChatSdk.Transport.Protocol;

namespace VyinChatSdk.Tests.Transport
{
    /// <summary>
    /// Unit tests for Command Protocol classes
    /// </summary>
    public class CommandProtocolTests
    {
        [Test]
        public void RequestIdGenerator_ShouldGenerateUniqueIds()
        {
            // Arrange
            RequestIdGenerator.Reset();

            // Act
            string id1 = RequestIdGenerator.Generate();
            string id2 = RequestIdGenerator.Generate();
            string id3 = RequestIdGenerator.Generate();

            // Assert
            Assert.AreNotEqual(id1, id2);
            Assert.AreNotEqual(id2, id3);
            Assert.AreNotEqual(id1, id3);
            Debug.Log($"[TEST] Generated IDs: {id1}, {id2}, {id3}");
        }

        [Test]
        public void LogiCommand_ShouldSerializeCorrectly()
        {
            // Arrange
            var command = new LogiCommand
            {
                UserId = "test_user",
                AccessToken = "test_token",
                ReqId = "req_123"
            };

            // Act
            string serialized = command.Serialize();

            // Assert
            Assert.IsTrue(serialized.StartsWith("LOGI"));
            Assert.IsTrue(serialized.Contains("test_user"));
            Assert.IsTrue(serialized.Contains("test_token"));
            Assert.IsTrue(serialized.Contains("req_123"));
            Debug.Log($"[TEST] Serialized: {serialized}");
        }

        [Test]
        public void CommandParser_ShouldExtractCommandType()
        {
            // Arrange
            string commandString = "LOGI{\"key\":\"value\"}";

            // Act
            string commandType = CommandParser.GetCommandType(commandString);

            // Assert
            Assert.AreEqual("LOGI", commandType);
        }

        [Test]
        public void CommandParser_ShouldExtractPayload()
        {
            // Arrange
            string commandString = "MESG{\"message\":\"hello world\"}";

            // Act
            string payload = CommandParser.GetPayload(commandString);

            // Assert
            Assert.AreEqual("{\"message\":\"hello world\"}", payload);
        }

        [Test]
        public void CommandParser_ShouldParseLogiResponse()
        {
            // Arrange
            string logiResponse = "LOGI{\"key\":\"session_abc123\",\"req_id\":\"req_456\"}";

            // Act
            var parsed = CommandParser.ParseLogiResponse(logiResponse);

            // Assert
            Assert.IsNotNull(parsed);
            Assert.AreEqual("session_abc123", parsed.SessionKey);
            Assert.AreEqual("req_456", parsed.ReqId);
        }

        [Test]
        public void CommandParser_ShouldHandleInvalidCommand()
        {
            // Arrange
            string invalidCommand = "XXX";

            // Act
            string payload = CommandParser.GetPayload(invalidCommand);

            // Assert
            Assert.IsNull(payload);
        }

        [Test]
        public void LogiResponse_ShouldParseError()
        {
            // Arrange
            string errorResponse = "LOGI{\"error\":\"Authentication failed\",\"error_code\":401}";

            // Act
            var parsed = CommandParser.ParseLogiResponse(errorResponse);

            // Assert
            Assert.IsNotNull(parsed);
            Assert.AreEqual("Authentication failed", parsed.Error);
            Assert.AreEqual(401, parsed.ErrorCode);
        }
    }
}
