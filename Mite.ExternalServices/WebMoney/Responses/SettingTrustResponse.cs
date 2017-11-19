using Mite.ExternalServices.WebMoney.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Responses
{
    public class SettingTrustResponse : WMResponse
    {
        [XmlElement("trust")]
        public TrustInfo TrustInfo { get; set; }
        [XmlElement("wmid")]
        public string WmId { get; set; }
    }
    public class TrustInfo
    {
        [XmlAttribute("purseid")]
        public string PurseId { get; set; }
        [XmlElement("realsmstype")]
        public byte LoginType { get; set; }
    }
}
