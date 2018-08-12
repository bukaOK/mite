using Mite.ExternalServices.Twitter.Responses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mite.ExternalServices.Twitter
{
    public class TwitterClient
    {
        private readonly string accessToken;
        private readonly HttpClient client;

        public TwitterClient(string accessToken, HttpClient client)
        {
            this.accessToken = accessToken;
            this.client = client;
        }
        public Task<TResponse> PostAsync<TResponse>(string url, object reqParams)
        {
            var dict = new Dictionary<string, string>();
            foreach(var prop in reqParams.GetType().GetProperties())
            {
                dict.Add(prop.Name, prop.GetValue(reqParams).ToString());
            }
            return PostAsync<TResponse>(url, dict);
        }
        public async Task<TResponse> PostAsync<TResponse>(string url, IDictionary<string, string> reqDict)
        {
            var formContent = new FormUrlEncodedContent(reqDict);
            var resp = await client.PostAsync(url, formContent);

            var respJson = JsonConvert.DeserializeObject<TResponse>(await resp.Content.ReadAsStringAsync());
            return respJson;
        }
        public Task<object> PostAsync(string method, object reqParams)
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in reqParams.GetType().GetProperties())
            {
                dict.Add(prop.Name, prop.GetValue(reqParams).ToString());
            }
            return PostAsync<object>(method, dict);
        }
        public async Task<object> PostAsync(string url, IDictionary<string, string> reqDict)
        {
            var formContent = new FormUrlEncodedContent(reqDict);
            var resp = await client.PostAsync(url, formContent);

            var respJson = JsonConvert.DeserializeObject(await resp.Content.ReadAsStringAsync());
            return respJson;
        }
        public async Task<UploadResponse> UploadMediaAsync(string filePath)
        {
            const string url = "https://upload.twitter.com/1.1/media/upload.json";
            var dataContent = new MultipartFormDataContent
            {
                { new StreamContent(File.OpenRead(filePath)), Path.GetFileNameWithoutExtension(filePath), Path.GetFileName(filePath) }
            };
            var resp = await client.PostAsync(url, dataContent);
            return JsonConvert.DeserializeObject<UploadResponse>(await resp.Content.ReadAsStringAsync());
        }
    }
}
