using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Enums
{
    /// <summary>
    /// Используемые сторонние сервисы
    /// </summary>
    public enum PaymentType : byte
    {
        /// <summary>
        /// Перевод с кошелька на кошелек (Яндекс)
        /// </summary>
        [Display(Name = "Яндекс.Кошелек")]
        YandexWallet,
        /// <summary>
        /// Перевод с банковской карты на Яндекс.Кошелек
        /// </summary>
        [Display(Name = "Банковская карта")]
        BankCard,
        WebMoney,
        QIWI
    }
}