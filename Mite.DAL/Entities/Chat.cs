using Mite.CodeData.Enums;
using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class Chat : GuidEntity
    {
        public string Name { get; set; }
        public string ImageSrc { get; set; }
        public string ImageSrcCompressed { get; set; }
        public int MaxMembersCount { get; set; }
        public ChatTypes Type { get; set; }
        [ForeignKey("Creator")]
        public string CreatorId { get; set; }
        public User Creator { get; set; }
        public List<ChatMember> Members { get; set; }
        public List<ChatMessage> Messages { get; set; }
    }
}
