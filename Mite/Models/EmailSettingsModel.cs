using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class EmailSettingsModel
    {
        public string Email { get; set; }
        public bool Confirmed { get; set; }
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Новый e-mail")]
        public string NewEmail { get; set; }
        [Required]
        [UIHint("TextBox")]
        [DataType(DataType.Password)]
        [DisplayName("Пароль от аккаунта")]
        public string Password { get; set; }
        public bool EmailConfirmationSended { get; set; }
    }
}