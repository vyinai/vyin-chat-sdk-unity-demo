// Domain/UseCases/GetChannelUseCase.cs
// Pure C# - No Unity dependencies (KMP-ready)

using System;
using System.Threading;
using System.Threading.Tasks;
using VyinChatSdk.Internal.Domain.Repositories;

namespace VyinChatSdk.Internal.Domain.UseCases
{
    /// <summary>
    /// Use case for getting a channel by URL
    /// Phase 3: Task 3.3, 3.4
    /// 100% Pure C#, no Unity dependencies (KMP-ready)
    /// </summary>
    public class GetChannelUseCase
    {
        private readonly IChannelRepository _channelRepository;

        public GetChannelUseCase(IChannelRepository channelRepository)
        {
            _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
        }

        /// <summary>
        /// Execute the use case - Get channel by URL
        /// </summary>
        /// <param name="channelUrl">Channel URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>VcGroupChannel object</returns>
        /// <exception cref="VcException">If validation fails or channel not found</exception>
        public async Task<VcGroupChannel> ExecuteAsync(
            string channelUrl,
            CancellationToken cancellationToken = default)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(channelUrl))
            {
                throw new VcException(
                    VcErrorCode.InvalidParameter,
                    "Channel URL cannot be null or empty",
                    "channelUrl");
            }

            // Execute repository call
            try
            {
                var channel = await _channelRepository.GetChannelAsync(channelUrl, cancellationToken);

                if (channel == null)
                {
                    throw new VcException(
                        VcErrorCode.ChannelNotFound,
                        $"Channel not found: {channelUrl}",
                        channelUrl);
                }

                return channel;
            }
            catch (VcException)
            {
                // Re-throw VcException as is
                throw;
            }
            catch (Exception ex)
            {
                // Wrap other exceptions in VcException
                throw new VcException(
                    VcErrorCode.Unknown,
                    "Failed to get channel",
                    channelUrl,
                    ex);
            }
        }
    }
}
