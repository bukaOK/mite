using Newtonsoft.Json;

namespace Mite.ExternalServices.DeviantArt.Responses
{
    public class AccessTokenResponse : Response
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("client_id")]
        public string ClientId { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
    }
}
