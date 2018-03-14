using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mite.ExternalServices.VkApi.Friends
{
    public class FriendsSearchResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("items")]
        public List<User> Users { get; set; }
    }
}
