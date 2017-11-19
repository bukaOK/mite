using Mite.ExternalServices.WebMoney.Core;
using Mite.ExternalServices.WebMoney.Responses;
using System;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Requests
{
    /// <summary>
    /// Интерфейс Х20(подтверждение запроса)
    /// </summary>
    [XmlRoot("merchant.request")]
    public class ExpressPaymentConfirmRequest : WMRequest
    {
        [XmlIgnore]
        public override string RequestUri => "https://merchant.webmoney.ru/conf/xml/XMLTransConfirm.asp";
        [XmlElement("lmi_payee_purse")]
        public string StorePurse { get; set; }
        [XmlElement("lmi_clientnumber")]
        public string ClientPhone { get; set; }
        [XmlElement("lmi_clientnumber_code")]
        public string ConfirmCode { get; set; }
        [XmlElement("lang")]
        public string Language { get; set; }
        [XmlElement("lmi_wminvoiceid")]
        public int InvoiceId { get; set; }

        public override string SignMessage => $"{WmId}{StorePurse}{InvoiceId}{ConfirmCode}";
    }
}
