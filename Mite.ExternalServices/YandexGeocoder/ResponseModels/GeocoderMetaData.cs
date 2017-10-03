using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class GeocoderMetaData
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("precision")]
        public string Precision { get; set; }
        public Address Address { get; set; }
        public AddressDetails AddressDetails { get; set; }
    }
}
