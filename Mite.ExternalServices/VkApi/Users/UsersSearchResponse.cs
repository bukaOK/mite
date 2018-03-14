using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mite.ExternalServices.VkApi.Users
{
    public class UsersSearchResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("items")]
        public List<User> Users { get; set; }
    }
}
