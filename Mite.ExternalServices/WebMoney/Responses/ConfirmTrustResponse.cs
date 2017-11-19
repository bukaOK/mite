using Mite.ExternalServices.WebMoney.Core;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Responses
{
    [XmlRoot("merchant.response")]
    public class ConfirmTrustResponse : WMResponse
    {
        [XmlElement("trust")]
        public ConfirmTrustInfo TrustInfo { get; set; }
        [XmlElement("smssentstate")]
        public string SmsStatus { get; set; }
    }
    public class ConfirmTrustInfo
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlElement("slavepurse")]
        public string ClientPurse { get; set; }
        [XmlElement("slavewmid")]
        public string ClientWmid { get; set; }
    }
}
