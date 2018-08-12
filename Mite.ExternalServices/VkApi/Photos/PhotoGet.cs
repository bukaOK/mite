using Mite.ExternalServices.VkApi.Core;
using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Photos
{
    public class PhotoGetRequest : VkRequest<PhotoGetResponse>
    {
        public PhotoGetRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        [VkParam("owner_id")]
        public string OwnerId { get; set; }
        [VkParam("album_id")]
        public string AlbumId { get; set; }
        [VkParam("extended")]
        public int Extended { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("count")]
        public int Count { get; set; }

        public override string Method => "photos.get";
    }
    public class PhotoGetResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("items")]
        public IEnumerable<Photo> Items { get; set; }
    }
}
