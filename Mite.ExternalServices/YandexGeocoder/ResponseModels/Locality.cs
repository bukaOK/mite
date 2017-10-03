using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class Locality
    {
        public string LocalityName { get; set; }
        public Thoroughfare Thoroughfare { get; set; }
    }
}
