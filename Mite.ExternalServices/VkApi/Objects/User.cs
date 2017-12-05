using Mite.ExternalServices.VkApi.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Objects
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("deactivated")]
        public string Deactivated { get; set; }
        [JsonProperty("hidden")]
        [JsonConverter(typeof(BoolConverter))]
        public bool? Hidden { get; set; }
        [JsonProperty("about")]
        public string About { get; set; }
        [JsonProperty("activities")]
        public string Activities { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
