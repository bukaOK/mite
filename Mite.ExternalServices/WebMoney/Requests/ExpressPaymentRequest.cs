using Mite.ExternalServices.WebMoney.Core;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Requests
{
    [XmlRoot("merchant.request")]
    public class ExpressPaymentRequest : WMRequest
    {
        public override string RequestUri => "https://merchant.webmoney.ru/conf/xml/XMLTransRequest.asp";
        [XmlElement("lmi_payment_no")]
        public int OrderId { get; set; }
        [XmlElement("lmi_payment_amount")]
        public double Amount { get; set; }
        [XmlElement("lmi_payment_desc")]
        public string Description { get; set; }
        [XmlElement("lmi_payee_purse")]
        public string StorePurse { get; set; }
        [XmlElement("lmi_clientnumber")]
        public string ClientPhone { get; set; }
        [XmlElement("lmi_clientnumber_type")]
        public byte LoginType { get; set; }
        [XmlElement("lmi_sms_type")]
        public byte ConfirmType { get; set; }
        [XmlElement("lang")]
        public string Language { get; set; }
        [XmlElement("emulated_flag")]
        public byte EmulatedFlag { get; set; }
        public override string SignMessage => $"{WmId}{StorePurse}{OrderId}{ClientPhone}{LoginType}";
    }
}
