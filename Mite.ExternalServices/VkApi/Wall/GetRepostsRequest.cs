using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Wall
{
    public class GetRepostsRequest : VkRequest<GetRepostsResponse>
    {
        [VkParam("owner_id")]
        public string OwnerId { get; set; }
        [VkParam("post_id")]
        public string PostId { get; set; }
        [VkParam("offset")]
        public int? Offset { get; set; }
        [VkParam("count")]
        public int? Count { get; set; }

        public override string Method => "wall.getReposts";

        public GetRepostsRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
    }
}
