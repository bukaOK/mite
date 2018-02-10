using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Board
{
    public class GetCommentsResponse
    {
        [JsonProperty("items")]
        public IEnumerable<BoardComment> Items { get; set; }
        [JsonProperty("profiles")]
        public IEnumerable<User> Profiles { get; set; }
    }
}
