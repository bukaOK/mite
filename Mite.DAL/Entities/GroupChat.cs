using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    public class GroupChat : GuidEntity
    {
        public string Name { get; set; }
        [ForeignKey("Creator")]
        public string CreatorId { get; set; }
        public User Creator { get; set; }
        public bool Open { get; set; }
        public IList<ChatMessage> Messages { get; set; }
        public IList<GroupChatMember> Members { get; set; }
    }
}
