using Mite.Attributes.DataAnnotations;
using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        [RegularExpression(@"[a-zA-Z0-9_]+", ErrorMessage = "Ник может содержать только английские буквы, цифры и знак подчеркивания")]
        [UIHint("TextBox")]
        [Required]
        public string NickName { get; set; }

        [DisplayName("Пол")]
        [UIHint("RadioButtonList")]
        [UIData(typeof(Genders))]
        [Required]
        public byte Gender { get; set; }
        
        [Required]
        [DisplayName("Показывать по умолчанию только подписки")]
        public bool ShowOnlyFollowings { get; set; }

        [DisplayName("Имя")]
        [UIHint("TextBox")]
        [RegularExpression(@"[А-Яа-яA-Za-z]*", ErrorMessage = "Имя может содержать только буквы")]
        public string FirstName { get; set; }

        [DisplayName("Фамилия")]
        [UIHint("TextBox")]
        [RegularExpression(@"[А-Яа-яA-Za-z]*", ErrorMessage = "Фамилия может содержать только буквы")]
        public string LastName { get; set; }

        [DisplayName("Возраст")]
        [Range(9, 80, ErrorMessage = "Слишком маленький/большой возраст")]
        [UIHint("TextBox")]
        public byte? Age { get; set; }
        /// <summary>
        /// Id города
        /// </summary>
        [DisplayName("Город")]
        public string City { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Cities { get; set; }
        /// <summary>
        /// Id страны
        /// </summary>
        [DisplayName("Страна")]
        public string Country { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Countries { get; set; }

        [DisplayName("Напишите о себе")]
        [UIHint("TextArea")]
        [MaxLength(150, ErrorMessage = "Слишком длинный текст для описания")]
        public string About { get; set; }
    }
    public class NotifySettingsModel
    {
        /// <summary>
        /// Заходил ли через вк
        /// </summary>
        public bool VkAuthenticated { get; set; }
        /// <summary>
        /// Уведомлять по почте
        /// </summary>
        [DisplayName("Уведомлять по почте")]
        [UIHint("Toggle")]
        public bool MailNotify { get; set; }
    }
    public class InviteSettingsModel
    {
        public string InviteKey { get; set; }
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
    public class ExternalLinkEditModel
    {
        public Guid? Id { get; set; }
        public string Url { get; set; }
    }
    public class ESPublishModel
    {
        public bool TwitterAuthenticated { get; set; }
        public bool DeviantArtAuthenticated { get; set; }

        //[DisplayName("Id страницы Facebook")]
        //public long FacebookPageId { get; set; }
        //[DisplayName("Id/короткое имя страницы ВКонтакте")]
        //public long VkGroupId { get; set; }

        //[DisplayName("Автоматически публиковать ВКонтакте")]
        //[UIHint("Checkbox")]
        //public bool PublishVk { get; set; }
        //[DisplayName("Автоматически публиковать на Facebook")]
        //[UIHint("Checkbox")]
        //public bool PublishFb { get; set; }
        [UIHint("Checkbox")]
        [DisplayName("Автоматически публиковать на DeviantArt")]
        public bool PublishDeviant { get; set; }
        [UIHint("Checkbox")]
        [DisplayName("Автоматически публиковать на Twitter")]
        public bool PublishTwitter { get; set; }
    }
}