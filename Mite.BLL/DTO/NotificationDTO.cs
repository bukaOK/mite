using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.BLL.DTO
{
    class NotificationDTO
    {
        public Guid Id { get; set; }
        public NotificationTypes NotificationType { get; set; }
        public bool IsNew { get; set; }
        /// <summary>
        /// Значение источника(имя пользователя, Id работы, Id комментария и пр.)
        /// </summary>
        public string SourceValue { get; set; }
        public DateTime NotifyDate { get; set; }
        public string UserId { get; set; }
        public string NotifyUserId { get; set; }
        /// <summary>
        /// Пользователь, который вызвал событие(например тот, кто оценил работу)
        /// </summary>
        public UserDTO NotifyUser { get; set; }
        /// <summary>
        /// Кому адресовано
        /// </summary>
        public UserDTO User { get; set; }
    }
}
