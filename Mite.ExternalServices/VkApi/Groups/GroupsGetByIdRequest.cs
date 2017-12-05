using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Mite.ExternalServices.VkApi.Objects;

namespace Mite.ExternalServices.VkApi.Groups
{
    public class GroupsGetByIdRequest : VkRequest<IEnumerable<Group>>
    {
        public GroupsGetByIdRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        [VkParam("group_ids")]
        public string GroupIds { get; set; }
        [VkParam("group_id")]
        public string GroupId { get; set; }
        [VkParam("fields")]
        public string Fields { get; set; }
        public override string Method => "groups.getById";
    }
}
