using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class Country
    {
        public string AddressLine { get; set; }
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }
        public string CountryNameCode { get; set; }
        public string CountryName { get; set; }
        public AdministrativeArea AdministrativeArea { get; set; }
    }
}
