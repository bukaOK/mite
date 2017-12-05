using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Mite.ExternalServices.VkApi.Objects;

namespace Mite.ExternalServices.VkApi.Users
{
    public class UsersGetRequest : VkRequest<IEnumerable<User>>
    {
        [VkParam("user_ids")]
        public string UserIds { get; set; }
        [VkParam("fields")]
        public string Fields { get; set; }
        [VkParam("name_case")]
        public string NameCase { get; set; }
        public override string Method => "users.get";
        public UsersGetRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
    }
}
