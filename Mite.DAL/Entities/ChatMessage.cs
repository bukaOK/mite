using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    public class ChatMessage : GuidEntity
    {
        /// <summary>
        /// Отправитель
        /// </summary>
        [ForeignKey("Sender")]
        public string SenderId { get; set; }
        public User Sender { get; set; }
        [ForeignKey("Chat")]
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        /// <summary>
        /// Содержание
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Дата отправки
        /// </summary>
        public DateTime SendDate { get; set; }
        /// <summary>
        /// Получатели(те, кто не удалил; отправитель тоже сюда входит)
        /// </summary>
        public IList<ChatMessageUser> Recipients { get; set; }
        public IList<ChatMessageAttachment> Attachments { get; set; }
        [MaxLength(32)]
        public string IV { get; set; }
    }
}
