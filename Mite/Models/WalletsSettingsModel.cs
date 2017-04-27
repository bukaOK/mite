using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    /// <summary>
    /// Настройки кошельков
    /// </summary>
    public class WalletsSettingsModel
    {
        /// <summary>
        /// Номер яндекс кошелька
        /// </summary>
        [Required]
        [DisplayName("Номер яндекс кошелька")]
        [UIHint("TextBox")]
        public string YandexWalId { get; set; }
        /// <summary>
        /// Пароль пользователя для проверки
        /// </summary>
        [Required]
        [DataType("password")]
        [DisplayName("Пароль от Mite аккаунта")]
        [UIHint("TextBox")]
        public string Password { get; set; }
    }
}