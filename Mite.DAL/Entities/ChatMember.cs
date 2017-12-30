using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    public class ChatMember
    {
        [Key, ForeignKey("User"), Column(Order = 0)]
        public string UserId { get; set; }
        public User User { get; set; }
        [Key, ForeignKey("Chat"), Column(Order = 1)]
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        [ForeignKey("Inviter")]
        public string InviterId { get; set; }
        public User Inviter { get; set; }
        /// <summary>
        /// Когда пользователь стал участником(по UTC)
        /// </summary>
        public DateTime? EnterDate { get; set; }
        /// <summary>
        /// Состояние участника чата
        /// </summary>
        public ChatMemberStatuses Status { get; set; }
    }
}
