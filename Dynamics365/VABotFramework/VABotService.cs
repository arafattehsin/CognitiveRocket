using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VABotFramework
{
    public class VABotService
    {
        private readonly HttpClient _httpClient;

        public VABotService(VABotEndpoint endpoint, string botName)
        {
            Endpoint = endpoint;
            BotName = botName;
            ChannelData = new VABotChannelData(endpoint.BotId, endpoint.TenantId);
            _httpClient = new HttpClient();
        }

        public string BotName { get; }

        public VABotChannelData ChannelData { get; }
        public VABotEndpoint Endpoint { get; }

        public async Task<string> GetTokenAsync()
        {
            var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = Endpoint.TokenUrl;
            var response = await _httpClient.SendAsync(httpRequest);
            var responseStr = await response.Content.ReadAsStringAsync();
            return SafeJsonConvert.DeserializeObject<DirectLineToken>(responseStr).token;
        }
    }

    public class VABotChannelData
    {
        public VABotChannelData(string botId, string tenantId)
        {
            DynamicsBotId = botId;
            DynamicsTenantId = tenantId;
        }

        // DO NOT CHANGE property name
        [JsonProperty("cci_bot_id")]
        public string DynamicsBotId { get; }

        // DO NOT CHANGE property name
        [JsonProperty("cci_tenant_id")]
        public string DynamicsTenantId { get; }
    }

    public class VABotEndpoint
    {
        public VABotEndpoint(string botId, string tenantId, string tokenEndPoint)
        {
            BotId = botId;
            TenantId = tenantId;
            UriBuilder uriBuilder = new UriBuilder(tokenEndPoint);
            uriBuilder.Query = $"botId={BotId}&tenantId={TenantId}";
            TokenUrl = uriBuilder.Uri;
        }

        public string BotId { get; }

        public string TenantId { get; }

        public Uri TokenUrl { get; }
    }

    public class DirectLineToken
    {
        public string token { get; set; }
    }
}
