using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Wall
{
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
