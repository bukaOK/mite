using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Board
{
    public class GetCommentsRequest : VkRequest<GetCommentsResponse>
    {
        public GetCommentsRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        [VkParam("group_id")]
        public string GroupId { get; set; }
        [VkParam("topic_id")]
        public string TopicId { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        [VkParam("need_likes")]
        public int NeedLikes { get; set; }
        [VkParam("sort")]
        public string Sort { get; set; }

        public override string Method => "board.getComments";
    }
}
