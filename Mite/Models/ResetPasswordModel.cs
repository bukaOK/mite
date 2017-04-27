using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class ResetPasswordModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        [UIHint("StrengthPass")]
        [MinLength(6, ErrorMessage = "Слишком короткий пароль")]
        [DisplayName("Новый пароль")]
        public string NewPass { get; set; }
        [Required]
        [DataType("password")]
        [DisplayName("Подтвердите пароль")]
        [Compare("NewPass", ErrorMessage = "Пароли должны совпадать")]
        [UIHint("TextBox")]
        public string ConfirmPass { get; set; }
    }
}