using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.DeviantArt.Responses
{
    public class StashSubmitResponse : Response
    {
        [JsonProperty("itemid")]
        public string ItemId { get; set; }
        [JsonProperty("stackid")]
        public string StackId { get; set; }
        [JsonProperty("stack")]
        public string Stack { get; set; }
    }
}
