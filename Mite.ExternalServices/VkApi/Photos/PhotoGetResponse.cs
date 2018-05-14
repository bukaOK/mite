using Mite.ExternalServices.VkApi.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mite.ExternalServices.VkApi.Photos
{
    public class PhotoGetResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("items")]
        public IEnumerable<Photo> Items { get; set; }
    }
}
