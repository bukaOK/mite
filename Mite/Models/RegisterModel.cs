using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mite.Attributes.DataAnnotations;
using Mite.Enums;

namespace Mite.Models
{
    public class RegisterModel
    {
        [Required]
        [DisplayName("Ник")]
        [RegularExpression(@"\w+", ErrorMessage = "Ник может содержать только английские буквы, цифры и знак подчеркивания")]
        [UIHint("TextBox")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        [DataType("text")]
        [DisplayName("E-mail")]
        [UIHint("TextBox")]
        public string Email { get; set; }
        [Required]
        [DataType("password")]
        [DisplayName("Пароль")]
        [UIHint("StrengthPass")]
        [MinLength(6, ErrorMessage = "Слишком короткий пароль")]
        public string Password { get; set; }
        [Required]
        [DataType("password")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DisplayName("Подтвердите пароль")]
        [UIHint("TextBox")]
        public string ConfirmPassword { get; set; }
    }
}