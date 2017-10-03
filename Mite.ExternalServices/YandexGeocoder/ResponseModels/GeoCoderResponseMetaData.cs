using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class GeocoderResponseMetaData
    {
        [JsonProperty("request")]
        public string Request { get; set; }
        [JsonProperty("found")]
        public string Found { get; set; }
        [JsonProperty("results")]
        public string Results { get; set; }
    }
}
