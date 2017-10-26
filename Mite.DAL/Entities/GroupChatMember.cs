using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class GroupChatMember : GuidEntity
    {
        [ForeignKey("User"), Required]
        public string UserId { get; set; }
        public User User { get; set; }
        [ForeignKey("Inviter")]
        public string InviterId { get; set; }
        public User Inviter { get; set; }
        [ForeignKey("Chat")]
        public Guid ChatId { get; set; }
        public GroupChat Chat { get; set; }
    }
}
