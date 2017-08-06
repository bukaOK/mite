using Mite.Attributes.DataAnnotations;
using Mite.Enums;
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
    public class ProfileSettingsModel
    {
        [DisplayName("Ник")]
        [RegularExpression(@"\w+", ErrorMessage = "Ник может содержать только английские буквы, цифры и знак подчеркивания")]
        [UIHint("TextBox")]
        [Required]
        public string NickName { get; set; }
        [DisplayName("Пол")]
        [UIHint("RadioButtonList")]
        [UIData(typeof(Genders))]
        [Required]
        public byte Gender { get; set; }
        [DisplayName("Имя")]
        [UIHint("TextBox")]
        [RegularExpression(@"[А-Яа-яA-Za-z].*", ErrorMessage = "Имя может содержать только буквы")]
        public string FirstName { get; set; }
        [DisplayName("Фамилия")]
        [UIHint("TextBox")]
        [RegularExpression(@"[А-Яа-яA-Za-z].*", ErrorMessage = "Фамилия может содержать только буквы")]
        public string LastName { get; set; }
        [DisplayName("Возраст")]
        [Range(9, 80, ErrorMessage = "Слишком маленький/большой возраст")]
        [UIHint("TextBox")]
        public byte? Age { get; set; }
        [DisplayName("Напишите о себе")]
        [UIHint("TextArea")]
        [MaxLength(150, ErrorMessage = "Слишком длинный текст для описания")]
        public string About { get; set; }
    }
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
    public class SocialLinksModel
    {
        [DisplayName("Вконтакте")]
        [UIHint("LabeledTextBox")]
        public string Vk { get; set; }
        [DisplayName("Твиттер")]
        [UIHint("LabeledTextBox")]
        public string Twitter { get; set; }
        [DisplayName("Facebook")]
        [UIHint("LabeledTextBox")]
        public string Facebook { get; set; }
        [DisplayName("Dribbble")]
        [UIHint("LabeledTextBox")]
        public string Dribbble { get; set; }
        [DisplayName("ArtStation")]
        [UIHint("LabeledTextBox")]
        public string ArtStation { get; set; }
        [DisplayName("Instagram")]
        [UIHint("LabeledTextBox")]
        public string Instagram { get; set; }
    }
}