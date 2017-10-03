using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Objects
{
    public class Error
    {
        [JsonProperty("error_code")]
        public int Code { get; set; }
        [JsonProperty("error_msg")]
        public string Message { get; set; }
    }
}
