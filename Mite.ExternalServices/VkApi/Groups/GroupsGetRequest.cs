using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Groups
{
    /// <summary>
    /// Достаем список сообществ пользователя
    /// </summary>
    public class GroupsGetRequest : VkRequest<GroupsGetResponse>
    {
        [VkParam("user_id")]
        public string UserId { get; set; }
        [VkParam("extended")]
        public int Extended { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        public override string Method => "groups.get";

        public GroupsGetRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
    }
}
