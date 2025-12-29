using Gamania.VyinChatSDK.Domain.Commands;
using NUnit.Framework;

namespace Gamania.VyinChatSDK.Tests.Domain.Commands
{
    public class CommandProtocolTests
    {
        [Test]
        public void SendCommand_ShouldGenerateUniqueReqId()
        {
            var protocol = new CommandProtocol();

            var c1 = protocol.BuildCommand(CommandType.MESG, new { text = "hi" });
            var c2 = protocol.BuildCommand(CommandType.MESG, new { text = "hi" });

            Assert.AreNotEqual(c1.reqId, c2.reqId);
        }

        [Test]
        public void SendCommand_ShouldSerialize_Correctly()
        {
            var protocol = new CommandProtocol();

            var cmd = protocol.BuildCommand(CommandType.MESG, new { text = "hello" });

            Assert.IsTrue(cmd.serialized.StartsWith("MESG{"));
            Assert.IsTrue(cmd.serialized.Contains("\"req_id\""));
            Assert.IsTrue(cmd.serialized.Contains("\"text\":\"hello\""));
        }
    }
}

