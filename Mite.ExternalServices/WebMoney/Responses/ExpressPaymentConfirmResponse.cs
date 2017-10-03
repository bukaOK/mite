using Mite.ExternalServices.WebMoney.Core;
using System;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Responses
{
    [XmlRoot("merchant.response")]
    public class ExpressPaymentConfirmResponse : WMResponse
    {
        public ExpressConfirmOperation Operation { get; set; }
    }
    public class ExpressConfirmOperation
    {
        [XmlElement("amount")]
        public int Amount { get; set; }
        [XmlElement("operdate")]
        public DateTime OperationDate { get; set; }
        [XmlElement("purpose")]
        public string Purpose { get; set; }
        [XmlElement("pursefrom")]
        public string ClientPurse { get; set; }
        [XmlElement("wmidfrom")]
        public string ClientWmid { get; set; }
        [XmlElement("wmtransid")]
        public int TransactionId { get; set; }
        [XmlElement("wminvoiceid")]
        public int InvoiceId { get; set; }
    }
}
