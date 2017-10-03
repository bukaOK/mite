using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    public enum DealStatuses : byte
    {
        /// <summary>
        /// Новые
        /// </summary>
        [Display(Name = "Новый")]
        New = 0,
        /// <summary>
        /// Ожидает оплаты
        /// </summary>
        [Display(Name = "Ожидает оплаты")]
        ExpectPayment = 4,
        /// <summary>
        /// Ожидает подтверждения клиента
        /// </summary>
        [Display(Name = "Ожидает подтверждения")]
        ExpectClient = 3,
        /// <summary>
        /// Спор(ожидает модератора)
        /// </summary>
        Dispute = 1,
        /// <summary>
        /// Отклонено модератором(в пользу клиента)
        /// </summary>
        Rejected = 6,
        /// <summary>
        /// Подтверждено клиентом(или модератором в пользу автора)
        /// </summary>
        [Display(Name = "Подтверждено")]
        Confirmed = 2
    }
}
