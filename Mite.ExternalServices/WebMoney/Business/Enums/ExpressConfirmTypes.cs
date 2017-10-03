using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.WebMoney.Business.Enums
{
    public enum ExpressConfirmTypes : byte
    {
        SMS = 1,
        USSD = 2,
        Auto = 3,
        BmPayment = 4
    }
}
