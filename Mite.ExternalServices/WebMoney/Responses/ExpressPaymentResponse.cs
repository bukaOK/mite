using Mite.ExternalServices.WebMoney.Core;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Responses
{
    [XmlRoot("merchant.response")]
    public class ExpressPaymentResponse : WMResponse
    {
        [XmlElement("operation")]
        public ExpressOperation Operation { get; set; }
    }
    public class ExpressOperation
    {
        [XmlAttribute("wminvoiceid")]
        public int InvoiceId { get; set; }
        [XmlElement("realsmstype")]
        public int SmsType { get; set; }
    }
}
