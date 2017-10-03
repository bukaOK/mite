using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.IpApi.Responses
{
    public class IpApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("contry")]
        public string Country { get; set; }
        [JsonProperty("contryCode")]
        public string CountryCode { get; set; }
        [JsonProperty("region")]
        public string RegionCode { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("lat")]
        public double Latitude { get; set; }
        [JsonProperty("lon")]
        public double Longitude { get; set; }
    }
}
