using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.ExternalServices.YandexGeocoder.ResponseModels
{
    public class GeoObjectCollection
    {
        public MetaDataProperty metaDataProperty { get; set; }
        public List<FeatureMember> featureMember { get; set; }
    }
}