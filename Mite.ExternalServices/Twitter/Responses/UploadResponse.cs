using Newtonsoft.Json;

namespace Mite.ExternalServices.Twitter.Responses
{
    public class UploadResponse
    {
        [JsonProperty("media_id_string")]
        public string MediaId { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("expires_after_secs")]
        public int ExpiresAfterSecs { get; set; }
        [JsonProperty("image")]
        public UploadImageInfo ImageInfo { get; set; }
    }
    public class UploadImageInfo
    {
        [JsonProperty("image_type")]
        public string ContentType { get; set; }
        [JsonProperty("w")]
        public int Width { get; set; }
        [JsonProperty("h")]
        public int Height { get; set; }
    }
}
