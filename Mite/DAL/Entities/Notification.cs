using Mite.DAL.Core;
using Mite.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    public class Notification : IEntity
    {
        public Guid Id { get; set; }
        public NotificationTypes NotificationType { get; set; }
        public bool IsNew { get; set; }
        /// <summary>
        /// Значение источника(имя пользователя, Id работы, Id комментария и пр.)
        /// </summary>
        public string SourceValue { get; set; }
        public DateTime NotifyDate { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [ForeignKey("NotifyUser")]
        public string NotifyUserId { get; set; }
        /// <summary>
        /// Пользователь, который вызвал событие(например тот, кто оценил работу)
        /// </summary>
        public User NotifyUser { get; set; }
        /// <summary>
        /// Кому адресовано
        /// </summary>
        public User User { get; set; }
    }
}