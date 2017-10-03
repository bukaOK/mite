using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class Address
    {
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }
        [JsonProperty("formated")]
        public string Formatted { get; set; }
        public IList<Component> Components { get; set; }
    }
}
