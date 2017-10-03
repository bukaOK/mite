using Mite.ExternalServices.IpApi.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.IpApi.Requests
{
    public class IpApiRequest
    {
        private readonly HttpClient httpClient;

        public IpApiRequest(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<IpApiResponse> PerformAsync(string ip)
        {
            var reqUri = "http://ip-api.com/json/" + ip;
            var resp = await httpClient.GetAsync(reqUri);
            var content = await resp.Content.ReadAsStringAsync();

            var ipResp = JsonConvert.DeserializeObject<IpApiResponse>(content);
            return ipResp;
        }
    }
}
