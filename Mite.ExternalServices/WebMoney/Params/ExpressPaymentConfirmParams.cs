using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.WebMoney.Params
{
    public class ExpressPaymentConfirmParams
    {
        public string ConfirmCode { get; set; }
        public int InvoiceId { get; set; }
    }
}
