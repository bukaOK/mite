using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Friends
{
    public class FriendsGetRequest : VkRequest<FriendsGetResponse>
    {
        public FriendsGetRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }

        [VkParam("user_id")]
        public string UserId { get; set; }
        [VkParam("order")]
        public string Order { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("fields")]
        public string Fields { get; set; }
        [VkParam("name_case")]
        public string NameCase { get; set; }

        public override string Method => "friends.get";
    }
}
