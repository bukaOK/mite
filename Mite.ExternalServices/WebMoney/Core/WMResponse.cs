using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Core
{
    public abstract class WMResponse
    {
        [XmlElement("retval")]
        public int ErrorCode { get; set; }
        [XmlElement("retdesc")]
        public string ErrorDescription { get; set; }
        [XmlElement("userdesc")]
        public string UserDescription { get; set; }
    }
}
