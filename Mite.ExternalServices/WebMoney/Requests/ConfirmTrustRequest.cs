using Mite.ExternalServices.WebMoney.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Requests
{
    public class ConfirmTrustRequest : WMRequest
    {
        public override string RequestUri => throw new NotImplementedException();
        public override string SignMessage => $"{WmId}{PurseId}{ConfirmCode}";
        [XmlElement("lmi_purseid")]
        public string PurseId { get; set; }
        [XmlElement("lmi_clientnumber_code")]
        public string ConfirmCode { get; set; }
    }
}
