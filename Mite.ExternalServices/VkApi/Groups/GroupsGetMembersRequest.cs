using Mite.ExternalServices.VkApi.Core;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Groups
{
    public class GroupsGetMembersRequest : VkRequest<GroupsGetMembersResponse>
    {
        public GroupsGetMembersRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        [VkParam("group_id")]
        public string GroupId { get; set; }
        [VkParam("sort")]
        public string Sort { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("filter")]
        public string Filter { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        public override string Method => "groups.getMembers";
    }
}
