using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Wall
{
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
