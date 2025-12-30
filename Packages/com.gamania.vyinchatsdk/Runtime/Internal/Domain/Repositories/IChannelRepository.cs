// Domain/Repositories/IChannelRepository.cs
// Pure C# - No Unity dependencies (KMP-ready)

using System.Threading;
using System.Threading.Tasks;

namespace VyinChatSdk.Internal.Domain.Repositories
{
    /// <summary>
    /// Channel repository interface
    /// Defines operations for channel management
    /// 100% Pure C#, no Unity dependencies (KMP-ready)
    /// </summary>
    public interface IChannelRepository
    {
        /// <summary>
        /// Get a channel by URL
        /// Phase 3: Task 3.3, 3.4
        /// </summary>
        /// <param name="channelUrl">Channel URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>VcGroupChannel object</returns>
        Task<VcGroupChannel> GetChannelAsync(
            string channelUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a group channel
        /// Phase 4: Task 4.1, 4.2
        /// </summary>
        /// <param name="createParams">Channel creation parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created VcGroupChannel</returns>
        Task<VcGroupChannel> CreateChannelAsync(
            VcGroupChannelCreateParams createParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a channel
        /// Future: Phase 7+
        /// </summary>
        Task<VcGroupChannel> UpdateChannelAsync(
            string channelUrl,
            VcGroupChannelUpdateParams updateParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a channel
        /// Future: Phase 7+
        /// </summary>
        Task DeleteChannelAsync(
            string channelUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Invite users to a channel
        /// Future: Phase 7+
        /// </summary>
        Task<VcGroupChannel> InviteUsersAsync(
            string channelUrl,
            string[] userIds,
            CancellationToken cancellationToken = default);
    }
}
