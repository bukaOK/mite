using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Messages
{
    /// <summary>
    /// Только если несколько 
    /// </summary>
    public class SendResponse
    {
        [JsonProperty("peer_id")]
        public string PeerId { get; set; }
        [JsonProperty("message_id")]
        public string MessageId { get; set; }
        public string Error { get; set; }
    }
}
