using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.WebMoney.Params
{
    public class TrustPaymentParams
    {
        public int RequestNum { get; set; }
        /// <summary>
        /// Кошелек, с которого снимаются деньги
        /// </summary>
        public string SourcePurse { get; set; }
        /// <summary>
        /// Куда начисляются
        /// </summary>
        public string TargetPurse { get; set; }
    }
}
