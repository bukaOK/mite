using Mite.ExternalServices.VkApi.Core;
using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Photos
{
    public class PhotoGetAll : VkRequest<PhotoGetAllResponse>
    {
        public PhotoGetAll(HttpClient httpClient, string token) : base(httpClient, token)
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
    public class PhotoGetAllResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("items")]
        public IEnumerable<Photo> Items { get; set; }
    }
}
