using NUnit.Framework;
using VyinChatSdk.Internal.Domain.Commands;

namespace VyinChatSdk.Tests.Internal.Domain.Commands
{
    public class CommandTypeExtensionsTests
    {
        [TestCase(CommandType.LOGI)]
        [TestCase(CommandType.MESG)]
        [TestCase(CommandType.FILE)]
        [TestCase(CommandType.EXIT)]
        [TestCase(CommandType.READ)]
        [TestCase(CommandType.MEDI)]
        [TestCase(CommandType.FEDI)]
        [TestCase(CommandType.ENTR)]
        [TestCase(CommandType.PEDI)]
        [TestCase(CommandType.VOTE)]
        [TestCase(CommandType.SUMM)]
        [TestCase(CommandType.MREV)]
        [TestCase(CommandType.FREV)]
        public void IsAckRequired_ShouldReturnTrue_ForAckCommands(CommandType commandType)
        {
            Assert.IsTrue(commandType.IsAckRequired());
        }

        [TestCase(CommandType.EROR)]
        [TestCase(CommandType.BRDM)]
        [TestCase(CommandType.ADMM)]
        [TestCase(CommandType.AEDI)]
        [TestCase(CommandType.TPST)]
        [TestCase(CommandType.TPEN)]
        [TestCase(CommandType.MTIO)]
        [TestCase(CommandType.SYEV)]
        [TestCase(CommandType.USEV)]
        [TestCase(CommandType.DELM)]
        [TestCase(CommandType.LEAV)]
        [TestCase(CommandType.UNRD)]
        [TestCase(CommandType.DLVR)]
        [TestCase(CommandType.NOOP)]
        [TestCase(CommandType.MRCT)]
        [TestCase(CommandType.PING)]
        [TestCase(CommandType.PONG)]
        [TestCase(CommandType.MACK)]
        [TestCase(CommandType.JOIN)]
        [TestCase(CommandType.MTHD)]
        [TestCase(CommandType.EXPR)]
        [TestCase(CommandType.MCNT)]
        [TestCase(CommandType.NONE)]
        [TestCase(CommandType.CUEV)]
        public void IsAckRequired_ShouldReturnFalse_ForNonAckCommands(CommandType commandType)
        {
            Assert.IsFalse(commandType.IsAckRequired());
        }
    }
}

