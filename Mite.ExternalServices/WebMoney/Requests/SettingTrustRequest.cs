using Mite.ExternalServices.WebMoney.Core;
using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Requests
{
    [XmlRoot("merchant.request")]
    public class SettingTrustRequest : WMRequest
    {
        public override string RequestUri => "https://merchant.webmoney.ru/conf/xml/XMLTrustRequest.asp";
        public override string SignMessage => $"{WmId}{StorePurse}{ClientPhone}{LoginType}{ConfirmType}";
        [XmlElement("lmi_day_limit")]
        public double DayLimit { get; set; }
        [XmlElement("lmi_payee_purse")]
        public string StorePurse { get; set; }
        [XmlElement("lmi_clientnumber")]
        public string ClientPhone { get; set; }
        [XmlElement("lmi_clientnumber_type")]
        public byte LoginType { get; set; }
        [XmlElement("lmi_sms_type")]
        public byte ConfirmType { get; set; }
    }
}
