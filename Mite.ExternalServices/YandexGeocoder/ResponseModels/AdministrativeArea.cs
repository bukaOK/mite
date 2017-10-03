using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class AdministrativeArea
    {
        public string AdministrativeAreaName { get; set; }
        public Locality Locality { get; set; }
    }
}
