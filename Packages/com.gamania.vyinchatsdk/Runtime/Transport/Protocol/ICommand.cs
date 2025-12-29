// -----------------------------------------------------------------------------
//
// Command Interface
//
// -----------------------------------------------------------------------------

namespace VyinChatSdk.Transport.Protocol
{
    /// <summary>
    /// Base interface for all commands
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Command type (e.g., "LOGI", "MESG", "ACK")
        /// </summary>
        string CommandType { get; }

        /// <summary>
        /// Serialize command to string format
        /// </summary>
        /// <returns>Serialized command string</returns>
        string Serialize();
    }
}
