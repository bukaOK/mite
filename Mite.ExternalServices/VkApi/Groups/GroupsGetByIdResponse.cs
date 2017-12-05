using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Groups
{
    public class GroupsGetByIdResponse
    {
        [JsonProperty("items")]
        public IEnumerable<Group> Groups { get; set; }
    }
}
