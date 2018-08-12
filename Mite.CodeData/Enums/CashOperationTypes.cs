using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Enums
{
    /// <summary>
    /// Типы перечисления д.е. внутри системы
    /// </summary>
    public enum CashOperationTypes : byte
    {
        /// <summary>
        /// Реклама Google(перечисление части доходов пользователю)
        /// </summary>
        GoogleAd = 0,
        /// <summary>
        /// Перечисление от реферала рефереру (комиссия за вывод денег)
        /// </summary>
        Referal = 1,
        /// <summary>
        /// Комиссия системы
        /// </summary>
        Comission = 2,
        /// <summary>
        /// Оплата сделки
        /// </summary>
        Deal = 3,
        /// <summary>
        /// Покупка товара
        /// </summary>
        Purchase = 4,
        /// <summary>
        /// Оплата платной подписки на автора
        /// </summary>
        TariffPay = 5
    }
}