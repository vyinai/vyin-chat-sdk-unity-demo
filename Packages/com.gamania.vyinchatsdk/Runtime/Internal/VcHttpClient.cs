// -----------------------------------------------------------------------------
//
// HTTP Client for VyinChat SDK (Pure C# Implementation)
//
// -----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace VyinChatSdk.Internal
{
    /// <summary>
    /// Pure C# HTTP client for VyinChat SDK REST API
    /// Used in Unity Editor and as fallback implementation
    /// </summary>
    internal class VcHttpClient
    {
        private readonly string appId;
        private readonly string domain;
        private readonly string userId;
        private string sessionToken;

        public VcHttpClient(string appId, string domain, string userId)
        {
            this.appId = appId;
            this.domain = domain;
            this.userId = userId;
        }

        public void SetSessionToken(string token)
        {
            this.sessionToken = token;
        }

        private string GetApiBaseUrl()
        {
            return $"https://{appId}.{domain}";
        }

        /// <summary>
        /// Create a group channel using REST API
        /// POST /v1/group_channels
        /// </summary>
        public IEnumerator CreateGroupChannel(VcGroupChannelCreateParams channelParams, Action<VcGroupChannel, string> callback)
        {
            string url = $"{GetApiBaseUrl()}/v1/group_channels";

            // Prepare request body
            var requestBody = new Dictionary<string, object>
            {
                { "name", channelParams.Name ?? "" },
                { "user_ids", channelParams.UserIds ?? new List<string>() },
                { "operator_ids", channelParams.OperatorUserIds ?? new List<string>() },
                { "is_distinct", channelParams.IsDistinct }
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Api-Token", appId);

                string sessionKey = "";
                request.SetRequestHeader("Session-Key", sessionKey);

                Debug.Log($"[VcHttpClient] Creating channel: {url}");
                Debug.Log($"[VcHttpClient] Request headers - Api-Token: {appId}, Session-Key: '{sessionKey}'");
                Debug.Log($"[VcHttpClient] Request body: {jsonBody}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"[VcHttpClient] Channel created successfully");
                    Debug.Log($"[VcHttpClient] Response: {responseText}");

                    try
                    {
                        var channel = ParseGroupChannelResponse(responseText);
                        callback?.Invoke(channel, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[VcHttpClient] Failed to parse response: {e}");
                        callback?.Invoke(null, $"Parse error: {e.Message}");
                    }
                }
                else
                {
                    string error = $"HTTP {request.responseCode}: {request.error}";
                    Debug.LogError($"[VcHttpClient] Failed to create channel: {error}");
                    Debug.LogError($"[VcHttpClient] Response: {request.downloadHandler.text}");
                    callback?.Invoke(null, error);
                }
            }
        }

        /// <summary>
        /// Send a user message using REST API
        /// POST /v1/group_channels/{channel_url}/messages
        /// </summary>
        public IEnumerator SendMessage(string channelUrl, string message, Action<VcBaseMessage, string> callback)
        {
            string url = $"{GetApiBaseUrl()}/v1/group_channels/{UnityWebRequest.EscapeURL(channelUrl)}/messages";

            // Prepare request body
            var requestBody = new Dictionary<string, object>
            {
                { "message_type", "MESG" },
                { "user_id", userId },
                { "message", message }
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Api-Token", appId);

                string sessionKey = "";
                request.SetRequestHeader("Session-Key", sessionKey);

                Debug.Log($"[VcHttpClient] Sending message: {url}");
                Debug.Log($"[VcHttpClient] Request headers - Api-Token: {appId}, Session-Key: '{sessionKey}'");
                Debug.Log($"[VcHttpClient] Request body: {jsonBody}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"[VcHttpClient] Message sent successfully");
                    Debug.Log($"[VcHttpClient] Response: {responseText}");

                    try
                    {
                        var baseMessage = ParseMessageResponse(responseText);
                        callback?.Invoke(baseMessage, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[VcHttpClient] Failed to parse response: {e}");
                        callback?.Invoke(null, $"Parse error: {e.Message}");
                    }
                }
                else
                {
                    string error = $"HTTP {request.responseCode}: {request.error}";
                    Debug.LogError($"[VcHttpClient] Failed to send message: {error}");
                    Debug.LogError($"[VcHttpClient] Response: {request.downloadHandler.text}");
                    callback?.Invoke(null, error);
                }
            }
        }

        private VcGroupChannel ParseGroupChannelResponse(string json)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            return new VcGroupChannel
            {
                ChannelUrl = response.ContainsKey("channel_url") ? response["channel_url"].ToString() : "",
                Name = response.ContainsKey("name") ? response["name"].ToString() : "",
                // Add more fields as needed
            };
        }

        private VcBaseMessage ParseMessageResponse(string json)
        {
            return JsonConvert.DeserializeObject<VcBaseMessage>(json);
        }
    }
}
