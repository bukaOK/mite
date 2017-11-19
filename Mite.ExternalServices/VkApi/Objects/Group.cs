using Mite.ExternalServices.VkApi.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Objects
{
    public class Group
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }
        [JsonProperty("is_closed")]
        public byte IsClosed { get; set; }
        [JsonProperty("deactivated")]
        public string Deactivated { get; set; }
        [JsonProperty("is_admin")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? IsAdmin { get; set; }
        [JsonProperty("admin_level")]
        public byte? AdminLevel { get; set; }
        [JsonProperty("is_member")]
        public bool IsMember { get; set; }
        [JsonProperty("invited_by")]
        public string InvitedBy { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("photo_50")]
        public string Photo50_Url { get; set; }
        [JsonProperty("photo_100")]
        public string Photo100_Url { get; set; }
        [JsonProperty("photo_200")]
        public string Photo200_Url { get; set; }
    }
}
