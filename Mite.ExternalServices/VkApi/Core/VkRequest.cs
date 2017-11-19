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
        private string VkParams
        {
            get
            {
                var props = GetType().GetProperties();
                var vkParams = props
                    .Where(prop => prop.GetCustomAttribute<VkParamAttribute>() != null && prop.GetValue(this) != null)
                    .Select(prop => $"{prop.GetCustomAttribute<VkParamAttribute>().Name}={prop.GetValue(this)}");
                return string.Join("&", vkParams);
            }
        }
        protected string Token { get; }
        public abstract string Method { get; }

        public VkRequest(HttpClient httpClient, string token)
        {
            Token = token;
            this.httpClient = httpClient;
        }

        public async Task<TResponse> PerformAsync()
        {
            var reqUri = new Uri($"https://api.vk.com/method/{Method}?{VkParams}&access_token={Token}&v={Version}");
            var resp = await httpClient.GetStringAsync(reqUri);
            var root = JObject.Parse(resp);
            if (root["error"] != null)
                throw new VkApiException((int)root["error"]["error_code"], (string)root["error"]["error_msg"]);
            var result = JsonConvert.DeserializeObject<TResponse>(root["response"].ToString());
            return result;
        }
    }
}
