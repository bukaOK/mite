using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class Component
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
