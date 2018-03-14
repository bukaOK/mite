using Mite.ExternalServices.VkApi.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Auth
{
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public string ExpiresSeconds { get; set; }
        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}
