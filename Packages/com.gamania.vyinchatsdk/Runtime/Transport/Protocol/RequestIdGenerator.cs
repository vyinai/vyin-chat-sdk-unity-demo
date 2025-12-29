// -----------------------------------------------------------------------------
//
// Request ID Generator
//
// -----------------------------------------------------------------------------

using System;
using System.Threading;

namespace VyinChatSdk.Transport.Protocol
{
    /// <summary>
    /// Generates unique request IDs for commands
    /// </summary>
    public class RequestIdGenerator
    {
        private static long _counter = 0;
        private static readonly object _lock = new object();

        /// <summary>
        /// Generate a unique request ID
        /// Format: timestamp_counter
        /// </summary>
        public static string Generate()
        {
            lock (_lock)
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long counter = Interlocked.Increment(ref _counter);
                return $"{timestamp}_{counter}";
            }
        }

        /// <summary>
        /// Reset counter (for testing purposes)
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _counter = 0;
            }
        }
    }
}
