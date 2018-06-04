using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    public enum QiwiBillStatuses
    {
        /// <summary>
        /// Счет выставлен, ожидает оплаты
        /// </summary>
        Waiting = 0,
        /// <summary>
        /// Счет оплачен
        /// </summary>
        Paid = 1,
        /// <summary>
        /// Счет отклонен
        /// </summary>
        Rejected = 2,
        /// <summary>
        /// Ошибка при проведении оплаты. Счет не оплачен
        /// </summary>
        Unpaid = 3,
        /// <summary>
        /// Время жизни счета истекло. Счет не оплачен
        /// </summary>
        Expired = 4
    }
}
