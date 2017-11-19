using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebMoney.Cryptography;

namespace Mite.ExternalServices.WebMoney.Core
{
    public abstract class WMRequest
    {
        public abstract string RequestUri { get; }
        public abstract string SignMessage { get; }
        [XmlElement("wmid")]
        public string WmId { get; set; }
        [XmlElement("sign")]
        public string Sign { get; set; }
        [XmlElement("lang")]
        public string Language { get; set; }
    }
}
