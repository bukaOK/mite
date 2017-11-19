using System.Xml.Serialization;

namespace Mite.ExternalServices.WebMoney.Core
{
    public abstract class WMResponse
    {
        /// <summary>
        /// Код ошибки
        /// </summary>
        [XmlElement("retval")]
        public int ErrorCode { get; set; }
        /// <summary>
        /// Описание ошибки
        /// </summary>
        [XmlElement("retdesc")]
        public string ErrorDescription { get; set; }
        /// <summary>
        /// Описание ошибки для пользователя
        /// </summary>
        [XmlElement("userdesc")]
        public string UserDescription { get; set; }
    }
}
