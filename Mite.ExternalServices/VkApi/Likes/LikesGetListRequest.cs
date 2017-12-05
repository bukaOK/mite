using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Likes
{
    public class LikesGetListRequest : VkRequest<LikesGetListResponse>
    {
        [VkParam("type")]
        public string Type { get; set; }
        [VkParam("item_id")]
        public string ItemId { get; set; }
        [VkParam("owner_id")]
        public string OwnerId { get; set; }
        [VkParam("page_url")]
        public string PageUrl { get; set; }
        [VkParam("filter")]
        public string Filter { get; set; }
        [VkParam("friends_only")]
        public int FriendsOnly { get; set; }
        [VkParam("extended")]
        public int Extended { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        [VkParam("skip_own")]
        public int SkipOwn { get; set; }

        public override string Method => "likes.getList";

        public LikesGetListRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
    }
}
