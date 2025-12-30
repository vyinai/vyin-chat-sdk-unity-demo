// Domain/UseCases/CreateChannelUseCase.cs
// Pure C# - No Unity dependencies (KMP-ready)

using System;
using System.Threading;
using System.Threading.Tasks;
using VyinChatSdk.Internal.Domain.Repositories;

namespace VyinChatSdk.Internal.Domain.UseCases
{
    /// <summary>
    /// Use case for creating a group channel
    /// Phase 4: Task 4.1, 4.2
    /// 100% Pure C#, no Unity dependencies (KMP-ready)
    /// </summary>
    public class CreateChannelUseCase
    {
        private readonly IChannelRepository _channelRepository;

        public CreateChannelUseCase(IChannelRepository channelRepository)
        {
            _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
        }

        /// <summary>
        /// Execute the use case - Create a group channel
        /// </summary>
        /// <param name="createParams">Channel creation parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created VcGroupChannel object</returns>
        /// <exception cref="VcException">If validation fails or creation fails</exception>
        public async Task<VcGroupChannel> ExecuteAsync(
            VcGroupChannelCreateParams createParams,
            CancellationToken cancellationToken = default)
        {
            // Validation
            if (createParams == null)
            {
                throw new VcException(
                    VcErrorCode.InvalidParameter,
                    "Channel creation parameters cannot be null",
                    "createParams");
            }

            if (createParams.UserIds == null || createParams.UserIds.Count == 0)
            {
                throw new VcException(
                    VcErrorCode.InvalidParameter,
                    "UserIds cannot be null or empty",
                    "createParams.UserIds");
            }

            // Execute repository call
            try
            {
                var channel = await _channelRepository.CreateChannelAsync(createParams, cancellationToken);

                if (channel == null)
                {
                    throw new VcException(
                        VcErrorCode.ChannelCreationFailed,
                        "Failed to create channel - repository returned null");
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
                    VcErrorCode.ChannelCreationFailed,
                    "Failed to create channel",
                    ex);
            }
        }
    }
}
