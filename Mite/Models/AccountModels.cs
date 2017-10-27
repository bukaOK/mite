using Mite.Attributes.DataAnnotations;
using Mite.Attributes.Validation;
using Mite.CodeData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class LoginModel
    {
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Ник")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Пароль")]
        [UIHint("TextBox")]
        public string Password { get; set; }
        [UIHint("Checkbox")]
        [DisplayName("Запомнить меня")]
        public bool Remember { get; set; }
    }
    public class ShortRegisterModel
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
        [UIHint("RadioButtonListOptional")]
        [UIData(typeof(RegisterRoles))]
        [DisplayName("Тип пользователя")]
        public byte? RegisterRole { get; set; }
    }
    public class RegisterModel
    {
        [Required]
        [DisplayName("Ник")]
        [RegularExpression(@"[a-zA-Z0-9-_]+", ErrorMessage = "Ник может содержать только английские буквы, цифры и знак подчеркивания")]
        [MaxLength(30, ErrorMessage = "Слишком длинный ник")]
        [UIHint("TextBox")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        [DataType("text")]
        [DisplayName("E-mail")]
        [UIHint("TextBox")]
        public string Email { get; set; }
        [Checked]
        [UIHint("RadioButtonListOptional")]
        [UIData(typeof(RegisterRoles))]
        [DisplayName("Тип пользователя")]
        public byte? RegisterRole { get; set; }
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
        public string RefererId { get; set; }
    }
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
    public class ForgotPassModel
    {
        [Required(ErrorMessage = "Введите E-mail")]
        [DisplayName("E-mail")]
        [UIHint("TextBox")]
        [EmailAddress]
        [DataType("text")]
        public string Email { get; set; }
    }
}