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
    /// <summary>
    /// Представляет собой доп. информацию о сообщении для каждого пользователя
    /// </summary>
    public class ChatMessageUser
    {
        [Key, ForeignKey("Message"), Column(Order = 0)]
        public Guid MessageId { get; set; }
        public ChatMessage Message { get; set; }
        [Key, ForeignKey("User"), Column(Order = 1)]
        public string UserId { get; set; }
        public User User { get; set; }
        /// <summary>
        /// Прочитал ли сообщение
        /// </summary>
        public bool Read { get; set; }
    }
}
