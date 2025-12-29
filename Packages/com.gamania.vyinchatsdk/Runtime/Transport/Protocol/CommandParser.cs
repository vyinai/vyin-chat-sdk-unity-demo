// -----------------------------------------------------------------------------
//
// Command Parser
//
// -----------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace VyinChatSdk.Transport.Protocol
{
    /// <summary>
    /// Parses incoming command strings
    /// </summary>
    public static class CommandParser
    {
        /// <summary>
        /// Extract command type from command string
        /// </summary>
        /// <param name="commandString">Raw command string (e.g., "LOGI{...}")</param>
        /// <returns>Command type (e.g., "LOGI")</returns>
        public static string GetCommandType(string commandString)
        {
            if (string.IsNullOrEmpty(commandString) || commandString.Length < 4)
                return null;

            // Command type is first 4 characters
            return commandString.Substring(0, 4);
        }

        /// <summary>
        /// Extract JSON payload from command string
        /// </summary>
        /// <param name="commandString">Raw command string (e.g., "LOGI{...}")</param>
        /// <returns>JSON payload string</returns>
        public static string GetPayload(string commandString)
        {
            if (string.IsNullOrEmpty(commandString) || commandString.Length <= 4)
                return null;

            return commandString.Substring(4);
        }

        /// <summary>
        /// Parse LOGI response
        /// </summary>
        public static LogiResponse ParseLogiResponse(string commandString)
        {
            string payload = GetPayload(commandString);
            if (string.IsNullOrEmpty(payload))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<LogiResponse>(payload);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
