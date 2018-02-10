using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.DTO
{
    public class PublicChatDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageSrc { get; set; }
        public string ImageSrcCompressed { get; set; }
        public int MaxMembersCount { get; set; }
        public int MembersCount { get; set; }
        public string CreatorId { get; set; }
    }
}
