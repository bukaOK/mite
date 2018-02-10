using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Objects
{
    public class BoardComment
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("from_id")]
        public string FromId { get; set; }
        [JsonProperty("date")]
        public int Date { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
