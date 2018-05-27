using Mite.ExternalServices.IpApi.Responses;
using Newtonsoft.Json;
using System.Net.Http;
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
            var resp = await httpClient.GetStringAsync(reqUri);

            var ipResp = JsonConvert.DeserializeObject<IpApiResponse>(resp);
            return ipResp;
        }
    }
}
