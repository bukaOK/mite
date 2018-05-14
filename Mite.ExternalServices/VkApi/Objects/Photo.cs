using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Objects
{
    public class Photo
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("album_id")]
        public string AlbumId { get; set; }
        [JsonProperty("owner_id")]
        public string OwnerId { get; set; }
        [JsonProperty("user_id")]
        public string LoadUserId { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("date")]
        public int Date { get; set; }
        [JsonProperty("photo_75")]
        public string Photo_75 { get; set; }
        [JsonProperty("photo_130")]
        public string Photo_130 { get; set; }
        [JsonProperty("photo_604")]
        public string Photo_604 { get; set; }
        [JsonProperty("photo_807")]
        public string Photo_807 { get; set; }
    }
}
