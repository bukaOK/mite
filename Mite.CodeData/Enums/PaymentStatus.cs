using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    public enum PaymentStatus : byte
    {
        /// <summary>
        /// Оплачено
        /// </summary>
        Payed = 0,
        /// <summary>
        /// Ожидает оплаты
        /// </summary>
        Waiting = 1
    }
}
