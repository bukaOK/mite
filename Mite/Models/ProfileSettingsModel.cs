using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Mite.Attributes.DataAnnotations;
using Mite.Core;
using Mite.Enums;

namespace Mite.Models
{
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
}