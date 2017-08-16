using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Mite.BLL.ExternalServices.Google.Core
{
    public class ApiRequest<TResult>
    {
        protected string AccessToken { get; }
        protected HttpClient HttpClient { get; }
        public IRequestParams RequestParams { get; set; }
        public string RequestUrl { get; set; }

        public ApiRequest(string accessToken, HttpClient httpClient)
        {
            AccessToken = accessToken;
            HttpClient = httpClient;

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        public async Task<TResult> GetAsync()
        {
            var reqParams = "?" + RequestParams.StringParams();
            var resp = await HttpClient.GetAsync(RequestUrl + reqParams);
            var result = await Parse(resp);
            return result;
        }
        public async Task<TResult> PostAsync()
        {
            var reqParams = new FormUrlEncodedContent(RequestParams.DictionaryParams());
            var resp = await HttpClient.PostAsync(RequestUrl, reqParams);
            var result = await Parse(resp);
            return result;
        }
        protected async Task<TResult> Parse(HttpResponseMessage resp)
        {
            return JsonConvert.DeserializeObject<TResult>(await resp.Content.ReadAsStringAsync());
        }
    }
}