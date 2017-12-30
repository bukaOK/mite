using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.DTO
{
    public class ShortChatDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageSrc { get; set; }
        public string ImageSrcCompressed { get; set; }
        public ChatTypes Type { get; set; }
        public ChatMessage LastMessage { get; set; }
    }
    public class ChatMemberDTO
    {
        public Guid ChatId { get; set; }
        public string UserId { get; set; }
        public int MembersCount { get; set; }
    }
    public class NewMessagesCountDTO
    {
        public Guid ChatId { get; set; }
        public int MessagesCount { get; set; }
    }
}
