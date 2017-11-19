using Mite.ExternalServices.WebMoney.Core;
using System;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Responses
{
    [XmlRoot("merchant.response")]
    public class ExpressPaymentConfirmResponse : WMResponse
    {
        [XmlElement("operation")]
        public ExpressConfirmOperation Operation { get; set; }
    }
    public class ExpressConfirmOperation
    {
        [XmlElement("amount")]
        public double Amount { get; set; }
        /// <summary>
        /// Формат даты такой - (yyyymmdd hh:MM:ss)
        /// </summary>
        [XmlElement("operdate")]
        public string OperationDate { get; set; }
        [XmlElement("purpose")]
        public string Purpose { get; set; }
        [XmlElement("pursefrom")]
        public string ClientPurse { get; set; }
        [XmlElement("wmidfrom")]
        public string ClientWmid { get; set; }
        [XmlAttribute("wmtransid")]
        public int TransactionId { get; set; }
        [XmlAttribute("wminvoiceid")]
        public int InvoiceId { get; set; }
    }
}
