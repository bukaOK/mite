using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class ChatMessageAttachmentsRepository : Repository<ChatMessageAttachment>
    {
        public ChatMessageAttachmentsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Есть ли у пользователя доступ к вложению
        /// </summary>
        /// <param name="attachmentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool HasAttachmentAccess(Guid attachmentId, string userId)
        {
            var query = "select count(*) from dbo.\"ChatMessageAttachments\" atts left outer join dbo.\"ChatMessages\" msgs " +
                "on msgs.\"Id\"=atts.\"MessageId\" left outer join dbo.\"ChatMessageUsers\" recip on recip.\"MessageId\"=msgs.\"Id\" " +
                "where recip.\"UserId\"=@userId and atts.\"Id\"=@attachmentId;";
            return Db.QueryFirst<int>(query, new { attachmentId, userId }) > 0;
        }
    }
}
