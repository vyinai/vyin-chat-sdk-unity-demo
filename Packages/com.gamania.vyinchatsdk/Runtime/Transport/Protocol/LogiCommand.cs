// -----------------------------------------------------------------------------
//
// LOGI Command (Login/Authentication)
//
// -----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace VyinChatSdk.Transport.Protocol
{
    /// <summary>
    /// LOGI command for authentication
    /// </summary>
    public class LogiCommand : ICommand
    {
        public string CommandType => "LOGI";

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("req_id")]
        public string ReqId { get; set; }

        public string Serialize()
        {
            var payload = new
            {
                user_id = UserId,
                access_token = AccessToken,
                req_id = ReqId
            };
            string json = JsonConvert.SerializeObject(payload);
            return $"{CommandType}{json}";
        }
    }

    /// <summary>
    /// LOGI response from server
    /// </summary>
    public class LogiResponse
    {
        [JsonProperty("key")]
        public string SessionKey { get; set; }

        [JsonProperty("req_id")]
        public string ReqId { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_code")]
        public int? ErrorCode { get; set; }
    }
}
