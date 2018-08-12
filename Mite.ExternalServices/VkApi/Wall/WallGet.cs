using Mite.ExternalServices.VkApi.Core;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Mite.ExternalServices.VkApi.Objects;

namespace Mite.ExternalServices.VkApi.Wall
{
    public class WallGetRequest : VkRequest<WallGetResponse>
    {
        public WallGetRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        [VkParam("owner_id")]
        public string OwnerId { get; set; }
        [VkParam("domain")]
        public string Domain { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        [VkParam("filter")]
        public string Filter { get; set; }
        [VkParam("extended")]
        public int Extended { get; set; }
        [VkParam("fields")]
        public string Fields { get; set; }
        public override string Method => "wall.get";
    }
    public class WallGetResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("items")]
        public IList<Post> Items { get; set; }
        [JsonProperty("profiles")]
        public IList<User> Profiles { get; set; }
    }
}
