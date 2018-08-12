using Mite.ExternalServices.VkApi.Core;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Mite.ExternalServices.VkApi.Objects;

namespace Mite.ExternalServices.VkApi.Wall
{
    public class GetReposts : VkRequest<GetRepostsResponse>
    {
        [VkParam("owner_id")]
        public string OwnerId { get; set; }
        [VkParam("post_id")]
        public string PostId { get; set; }
        [VkParam("offset")]
        public int? Offset { get; set; }
        [VkParam("count")]
        public int? Count { get; set; }

        public override string Method => "wall.getReposts";

        public GetReposts(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
    }
    public class GetRepostsResponse
    {
        [JsonProperty("items")]
        public IList<Post> Items { get; set; }
        [JsonProperty("profiles")]
        public IList<User> Profiles { get; set; }
        [JsonProperty("groups")]
        public IList<Group> Groups { get; set; }
    }
}
