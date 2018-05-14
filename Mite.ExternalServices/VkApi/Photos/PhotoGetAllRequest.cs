using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Photos
{
    public class PhotoGetAllRequest : VkRequest<PhotoGetAllResponse>
    {
        public PhotoGetAllRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        [VkParam("owner_id")]
        public string OwnerId { get; set; }
        [VkParam("extended")]
        public int Extended { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        [VkParam("photo_sizes")]
        public int? PhotoSizes { get; set; }
        public override string Method => "photos.getAll";
    }
}
