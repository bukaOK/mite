using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        private IDictionary<string, string> DictVkParams
        {
            get
            {
                var props = GetType().GetProperties();
                return props
                    .Where(prop => prop.GetCustomAttribute<VkParamAttribute>() != null && prop.GetValue(this) != null)
                    .ToDictionary(prop => prop.GetCustomAttribute<VkParamAttribute>().Name, prop => prop.GetValue(this).ToString());
            }
        }
        protected string Token { get; }
        public abstract string Method { get; }
        private HttpMethod httpMethod;
        public HttpMethod HttpMethod
        {
            get { return httpMethod ?? HttpMethod.Get; }
            set { httpMethod = value; }
        }

        public VkRequest(HttpClient httpClient, string token)
        {
            Token = token;
            this.httpClient = httpClient;
        }

        public async Task<TResponse> PerformAsync()
        {
            string resp;
            if(HttpMethod == HttpMethod.Get)
            {
                var reqUri = new Uri($"https://api.vk.com/method/{Method}?{VkParams}&access_token={Token}&v={Version}");
                resp = await httpClient.GetStringAsync(reqUri);
            }
            else if(HttpMethod == HttpMethod.Post)
            {
                var reqUri = new Uri($"https://api.vk.com/method/{Method}");
                var fParams = DictVkParams;
                fParams.Add("access_token", Token);
                fParams.Add("v", Version);
                var data = new FormUrlEncodedContent(fParams);
                resp = await (await httpClient.PostAsync(reqUri, data)).Content.ReadAsStringAsync();
            }
            else
            {
                throw new ArgumentException("Метод не поддерживается");
            }
            var root = JObject.Parse(resp);
            if (root["error"] != null)
                throw new VkApiException((int)root["error"]["error_code"], (string)root["error"]["error_msg"]);
            var result = JsonConvert.DeserializeObject<TResponse>(root["response"].ToString());
            return result;
        }
    }
}
