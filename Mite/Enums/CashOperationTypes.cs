using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Enums
{
    public enum CashOperationTypes : byte
    {
        /// <summary>
        /// Реклама
        /// </summary>
        Ad,
        /// <summary>
        /// Перечисление от реферала рефереру (комиссия за вывод денег)
        /// </summary>
        Referal,
        /// <summary>
        /// Комиссия системы
        /// </summary>
        Comission
    }
}