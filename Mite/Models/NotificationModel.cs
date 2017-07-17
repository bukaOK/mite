using Mite.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class NotificationModel
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Пользователь, который вызвал событие(например тот, кто оценил работу)
        /// </summary>
        public UserShortModel NotifyUser { get; set; }
        /// <summary>
        /// Кому адресовано
        /// </summary>
        public UserShortModel User { get; set; }
        [Required]
        public NotificationTypes NotificationType { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Значение источника(имя пользователя, Id работы, Id комментария и пр.)
        /// </summary>
        public string SourceValue { get; set; }
        public bool IsNew { get; set; }
        public DateTime NotifyDate { get; set; }
        public string NotifyDateStr { get; set; }
    }
}