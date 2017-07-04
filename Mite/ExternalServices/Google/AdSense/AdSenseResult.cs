using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.ExternalServices.Google.AdSense
{
    public class AdSenseReportsResult
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("totalMatchedRows")]
        public int TotalMatchedRows { get; set; }
        [JsonProperty("headers")]
        public IEnumerable<Header> Headers { get; set; }
        [JsonProperty("rows")]
        public IEnumerable<string> Rows { get; set; }
        [JsonProperty("totals")]
        public IEnumerable<string> Totals { get; set; }
        [JsonProperty("averages")]
        public IEnumerable<string> Averages { get; set; }
        [JsonProperty("warnings")]
        public IEnumerable<string> Warnings { get; set; }

        public class Header
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("currency")]
            public string Currency { get; set; }
        }
    }
}