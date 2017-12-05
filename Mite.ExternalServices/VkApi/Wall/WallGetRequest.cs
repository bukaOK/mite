using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Wall
{
    public class WallGetRequest : VkRequest<WallGetResponse>
    {
        public WallGetRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        [VkParam("owner_id")]
        public string OwnerId { get; set; }
        [VkParam("domain")]
        public string Domain { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        [VkParam("filter")]
        public string Filter { get; set; }
        [VkParam("extended")]
        public int Extended { get; set; }
        [VkParam("fields")]
        public string Fields { get; set; }
        public override string Method => "wall.get";
    }
}
