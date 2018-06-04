using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mite.Models
{
    public class FaqQuestionModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("header")]
        public string Header { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("items")]
        public IEnumerable<string> Items { get; set; }
    }
}