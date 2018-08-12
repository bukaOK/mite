using Mite.ExternalServices.VkApi.Core;
using Newtonsoft.Json;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Photos
{
    public class PhotoGetWallUploadServerRequest : VkRequest<PhotoGetWallUploadServerResponse>
    {
        public PhotoGetWallUploadServerRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }

        [VkParam("group_id")]
        public string GroupId { get; set; }
        public override string Method => "photos.getWallUploadServer";
    }
    public class PhotoGetWallUploadServerResponse
    {
        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }
        [JsonProperty("album_id")]
        public string AlbumId { get; set; }
        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}
