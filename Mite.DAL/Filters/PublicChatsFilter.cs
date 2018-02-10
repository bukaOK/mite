using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Filters
{
    public class PublicChatsFilter
    {
        public string UserId { get; set; }
        public string Input { get; set; }
        public int Range { get; set; }
        public int Offset { get; set; }
        public ChatTypes PublicType => ChatTypes.Public;
    }
}
