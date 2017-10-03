using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Core
{
    public abstract class VkRequest<TResponse>
    {
        private const string Version = "5.68";

        private readonly HttpClient httpClient;
        private readonly Uri reqUri;

        private string VkParams
        {
            get
            {
                var props = GetType().GetProperties();
                var vkParams = props
                    .Where(prop => prop.GetCustomAttribute<VkParamAttribute>() != null && prop.GetValue(this) != null)
                    .Select(prop => prop.GetCustomAttribute<VkParamAttribute>());
                return string.Join("&", vkParams);
            }
        }
        public abstract string Method { get; }

        public VkRequest(HttpClient httpClient, string token)
        {
            reqUri = new Uri($"https://api.vk.com/method/{Method}?{VkParams}&access_token={token}&v={Version}");
            this.httpClient = httpClient;
            this.httpClient = httpClient;
        }

        public async Task<TResponse> PerformAsync()
        {
            var resp = await httpClient.GetAsync(reqUri);
            var content = await resp.Content.ReadAsStringAsync();
            var root = JObject.Parse(content);
            if (root["error"] != null)
                throw new VkApiException((int)root["error"]["error_code"], (string)root["error"]["error_msg"]);
            var result = JsonConvert.DeserializeObject<TResponse>((string)root["response"]);
            return result;
        }
    }
}
