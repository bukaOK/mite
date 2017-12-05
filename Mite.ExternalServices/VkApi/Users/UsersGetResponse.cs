using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Users
{
    public class UsersGetResponse
    {
        [JsonProperty("response")]
        public IEnumerable<User> Response { get; set; }
    }
}
