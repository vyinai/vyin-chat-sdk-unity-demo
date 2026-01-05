using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VyinChatSdk.Internal.Data.DTOs;
using VyinChatSdk.Internal.Data.Mappers;
using VyinChatSdk.Internal.Data.Network;
using VyinChatSdk.Internal.Domain.Models;
using VyinChatSdk.Internal.Domain.Repositories;

namespace VyinChatSdk.Internal.Data.Repositories
{
    /// <summary>
    /// Default implementation of channel repository
    /// </summary>
    public class ChannelRepositoryImpl : IChannelRepository
    {
        private readonly IHttpClient _httpClient;
        private readonly string _baseUrl;

        public ChannelRepositoryImpl(IHttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<ChannelBO> GetChannelAsync(
            string channelUrl,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(async () =>
            {
                var url = $"{_baseUrl}/group_channels/{channelUrl}";
                var response = await _httpClient.GetAsync(url, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                var channelDto = JsonConvert.DeserializeObject<ChannelDTO>(response.Body);
                return ChannelDtoMapper.ToBusinessObject(channelDto);
            }, "Failed to get channel", channelUrl);
        }

        public async Task<ChannelBO> CreateChannelAsync(
            VcGroupChannelCreateParams createParams,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(async () =>
            {
                var url = $"{_baseUrl}/group_channels";
                var requestBody = JsonConvert.SerializeObject(new
                {
                    user_ids = createParams.UserIds,
                    operator_user_ids = createParams.OperatorUserIds,
                    name = createParams.Name,
                    cover_url = createParams.CoverUrl,
                    custom_type = createParams.CustomType,
                    data = createParams.Data,
                    is_distinct = createParams.IsDistinct
                });

                var response = await _httpClient.PostAsync(url, requestBody, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                var channelDto = JsonConvert.DeserializeObject<ChannelDTO>(response.Body);
                return ChannelDtoMapper.ToBusinessObject(channelDto);
            }, "Failed to create channel");
        }

        public async Task<ChannelBO> UpdateChannelAsync(
            string channelUrl,
            VcGroupChannelUpdateParams updateParams,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(async () =>
            {
                var url = $"{_baseUrl}/group_channels/{channelUrl}";
                var requestBody = JsonConvert.SerializeObject(new
                {
                    name = updateParams.Name,
                    cover_url = updateParams.CoverUrl,
                    custom_type = updateParams.CustomType,
                    data = updateParams.Data
                });

                var response = await _httpClient.PutAsync(url, requestBody, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                var channelDto = JsonConvert.DeserializeObject<ChannelDTO>(response.Body);
                return ChannelDtoMapper.ToBusinessObject(channelDto);
            }, "Failed to update channel", channelUrl);
        }

        public async Task DeleteChannelAsync(
            string channelUrl,
            CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(async () =>
            {
                var url = $"{_baseUrl}/group_channels/{channelUrl}";
                var response = await _httpClient.DeleteAsync(url, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }
            }, "Failed to delete channel", channelUrl);
        }

        public async Task<ChannelBO> InviteUsersAsync(
            string channelUrl,
            string[] userIds,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(async () =>
            {
                var url = $"{_baseUrl}/group_channels/{channelUrl}/invite";
                var requestBody = JsonConvert.SerializeObject(new { user_ids = userIds });

                var response = await _httpClient.PostAsync(url, requestBody, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                var channelDto = JsonConvert.DeserializeObject<ChannelDTO>(response.Body);
                return ChannelDtoMapper.ToBusinessObject(channelDto);
            }, "Failed to invite users", channelUrl);
        }

        private async Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation,
            string errorMessage,
            string context = null)
        {
            try
            {
                return await operation();
            }
            catch (VcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new VcException(VcErrorCode.NetworkError, errorMessage, context, ex);
            }
        }

        private async Task ExecuteAsync(
            Func<Task> operation,
            string errorMessage,
            string context = null)
        {
            try
            {
                await operation();
            }
            catch (VcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new VcException(VcErrorCode.NetworkError, errorMessage, context, ex);
            }
        }

        private VcException CreateExceptionFromResponse(HttpResponse response)
        {
            return response.StatusCode switch
            {
                400 => new VcException(VcErrorCode.InvalidParameterValue, "Invalid request parameters", response.Body),
                401 => new VcException(VcErrorCode.InvalidSessionKey, "Invalid or missing session key", response.Body),
                403 => new VcException(VcErrorCode.InvalidSessionKey, "Invalid session key", response.Body),
                404 => new VcException(VcErrorCode.ChannelNotFound, "Channel not found", response.Body),
                500 => new VcException(VcErrorCode.InternalServerError, "Internal server error", response.Body),
                _ => new VcException(VcErrorCode.NetworkError, $"HTTP {response.StatusCode}: {response.Error}", response.Body)
            };
        }
    }
}
