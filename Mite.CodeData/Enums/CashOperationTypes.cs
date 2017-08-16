using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Enums
{
    public enum CashOperationTypes : byte
    {
        /// <summary>
        /// Реклама Google(перечисление части доходов пользователю)
        /// </summary>
        GoogleAd,
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