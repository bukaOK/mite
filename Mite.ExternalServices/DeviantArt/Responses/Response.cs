using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.DeviantArt.Responses
{
    public class Response
    {
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
        [JsonProperty("error_details")]
        public string ErrorDetails { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
