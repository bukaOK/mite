using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Friends
{
    public class FriendsSearchRequest : VkRequest<FriendsSearchResponse>
    {
        public FriendsSearchRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }

        [VkParam("user_id")]
        public string UserId { get; set; }
        [VkParam("q")]
        public string Query { get; set; }
        [VkParam("fields")]
        public string Fields { get; set; }
        [VkParam("name_case")]
        public string NameCase { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("count")]
        public int Count { get; set; }

        public override string Method => "friends.search";
    }
}
