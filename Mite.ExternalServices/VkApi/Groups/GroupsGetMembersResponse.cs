using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mite.ExternalServices.VkApi.Groups
{
    public class GroupsGetMembersResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("items")]
        public IEnumerable<string> Members { get; set; }
    }
}
