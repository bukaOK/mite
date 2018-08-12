using Mite.ExternalServices.DeviantArt.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mite.ExternalServices.DeviantArt
{
    public class DeviantArtClient
    {
        public string AccessToken { get; set; }

        private readonly HttpClient client;

        public DeviantArtClient(string accessToken, HttpClient client)
        {
            AccessToken = accessToken;
            this.client = client;
        }
        public DeviantArtClient(HttpClient client)
        {
            this.client = client;
        }

        public Task<TResponse> GetAsync<TResponse>(string url, object reqParams) where TResponse : Response
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in reqParams.GetType().GetProperties())
            {
                dict.Add(prop.Name, prop.GetValue(reqParams).ToString().ToLower());
            }
            return PostAsync<TResponse>(url, dict);
        }

        public async Task<TResponse> GetAsync<TResponse>(string url, IDictionary<string, string> reqDict) where TResponse : Response
        {
            if (!string.IsNullOrEmpty(AccessToken))
                reqDict.Add("access_token", AccessToken);

            var reqParams = string.Join("&", reqDict.Select(x => $"{x.Key}={x.Value}"));
            var resp = await client.GetAsync($"{url}?{reqParams}");

            return await ParseResponse<TResponse>(resp);
        }

        public Task<TResponse> PostAsync<TResponse>(string url, object reqParams) where TResponse : Response
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in reqParams.GetType().GetProperties())
            {
                dict.Add(prop.Name, prop.GetValue(reqParams).ToString().ToLower());
            }
            return PostAsync<TResponse>(url, dict);
        }

        public async Task<TResponse> PostAsync<TResponse>(string url, IList<KeyValuePair<string, string>> reqParams) where TResponse : Response
        {
            if (!string.IsNullOrEmpty(AccessToken))
                reqParams.Add(new KeyValuePair<string, string>("access_token", AccessToken));

            var formContent = new FormUrlEncodedContent(reqParams);
            var resp = await client.PostAsync(url, formContent);

            return await ParseResponse<TResponse>(resp);
        }

        public Task<TResponse> PostUploadAsync<TResponse>(string url, string filePath, object reqParams) where TResponse : Response
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in reqParams.GetType().GetProperties())
            {
                dict.Add(prop.Name, prop.GetValue(reqParams).ToString());
            }
            return PostUploadAsync<TResponse>(url, filePath, dict);
        }

        public async Task<TResponse> PostUploadAsync<TResponse>(string url, string filePath, IList<KeyValuePair<string, string>> reqParams) 
            where TResponse : Response
        {
            var content = new MultipartFormDataContent("deviapi---" + DateTime.UtcNow.Ticks.ToString("x"))
            {
                { new StreamContent(File.OpenRead(filePath)), Path.GetFileName(filePath), Path.GetFileNameWithoutExtension(filePath) }
            };
            if (!string.IsNullOrEmpty(AccessToken))
                reqParams.Add(new KeyValuePair<string, string>("access_token", AccessToken));

            foreach (var pair in reqParams)
            {
                content.Add(new StringContent(pair.Value), pair.Key);
            }
            var resp = await client.PostAsync(url, content);
            return await ParseResponse<TResponse>(resp);
        }

        private async Task<TResponse> ParseResponse<TResponse>(HttpResponseMessage resp) where TResponse : Response
        {
            if (resp.IsSuccessStatusCode)
            {
                var respJson = JsonConvert.DeserializeObject<TResponse>(await resp.Content.ReadAsStringAsync());
                if (respJson.Status == "success")
                    return respJson;
                throw new DeviantArtException($"{respJson.Error}: {respJson.ErrorDescription}");
            }
            if(resp.Content.Headers.ContentType.MediaType == "application/json")
            {
                var respJson = JsonConvert.DeserializeObject<Response>(await resp.Content.ReadAsStringAsync());
                throw new DeviantArtException($"{respJson.Error}: {respJson.ErrorDescription}");
            }
            throw new DeviantArtException(resp.StatusCode);
        }
    }
}
