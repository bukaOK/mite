using Mite.CodeData.Enums;
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
    public class Chat : GuidEntity
    {
        public IList<User> Members { get; set; }
        public IList<ChatMessage> Messages { get; set; }
    }
}
