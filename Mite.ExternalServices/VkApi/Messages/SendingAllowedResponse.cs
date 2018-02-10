using Newtonsoft.Json;

namespace Mite.ExternalServices.VkApi.Messages
{
    public class SendingAllowedResponse
    {
        [JsonProperty("is_allowed")]
        public bool Allowed { get; set; }
    }
}
