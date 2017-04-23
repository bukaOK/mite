using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class ChangePassModel
    {
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Старый пароль")]
        [DataType(DataType.Password)]
        public string OldPass { get; set; }
        [Required]
        [UIHint("StrengthPass")]
        [MinLength(6, ErrorMessage = "Слишком короткий пароль")]
        [MaxLength(100, ErrorMessage = "Слишком длинный пароль")]
        [DisplayName("Новый пароль")]
        public string NewPass { get; set; }
        [Required]
        [DataType("password")]
        [DisplayName("Подтвердите пароль")]
        [Compare("NewPass", ErrorMessage = "Пароли не совпадают")]
        [UIHint("TextBox")]
        public string ConfirmPass { get; set; }
    }
}