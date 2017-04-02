using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class ResetPasswordModel
    {
        public string Code { get; set; }
        public string UserId { get; set; }
        [Required]
        [UIHint("StrengthPass")]
        [MinLength(6, ErrorMessage = "Слишком короткий пароль")]
        [MaxLength(100, ErrorMessage = "Слишком длинный пароль")]
        [DisplayName("Новый пароль")]
        public string NewPass { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Подтвердите пароль")]
        [Compare("NewPass", ErrorMessage = "Пароли должны совпадать")]
        public string ConfirmPass { get; set; }
    }
}