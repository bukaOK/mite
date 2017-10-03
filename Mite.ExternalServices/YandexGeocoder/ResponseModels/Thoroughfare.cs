using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class Thoroughfare
    {
        public string ThoroughfareName { get; set; }
        public Premise Premise { get; set; }
    }
}
