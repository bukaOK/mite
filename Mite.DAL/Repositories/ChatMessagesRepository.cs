using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Dapper;
using System.Data.Entity;
using System.Data;

namespace Mite.DAL.Repositories
{
    public class ChatMessagesRepository : Repository<ChatMessage>
    {
        public ChatMessagesRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public Task ReadAsync(Guid messageId, string userId)
        {
            var query = "update dbo.\"ChatMessageUsers\" set \"Read\"=true where \"MessageId\"=@messageId and \"UserId\"=@userId";
            return Db.ExecuteAsync(query, new { messageId, userId });
        }
        public void Read(Guid messageId, string userId)
        {
            var query = "update dbo.\"ChatMessageUsers\" set \"Read\"=true where \"MessageId\"=@messageId and \"UserId\"=@userId";
            Db.Execute(query, new { messageId, userId });
        }
        public void ReadUnreaded(Guid chatId, string userId)
        {
            if (Db.State != ConnectionState.Closed)
                return;
            var query = "select messages.\"Id\" from dbo.\"ChatMessageUsers\" as message_users inner join dbo.\"ChatMessages\" as messages " +
                "on messages.\"Id\"=message_users.\"MessageId\" where messages.\"ChatId\"=@chatId and message_users.\"UserId\"=@userId " +
                "and message_users.\"Read\"=false group by messages.\"Id\";";
            var ids = Db.Query<Guid>(query, new { chatId, userId });
            query = "update dbo.\"ChatMessageUsers\" set \"Read\"=true where \"MessageId\"=any(@ids) and \"UserId\"=@userId;";
            Db.Execute(query, new { ids, userId });
        }
        public Task RemoveAsync(Guid messageId, string userId)
        {
            var query = "delete from dbo.\"ChatMessageUsers\" where MessageId=@messageId and UserId=@userId";
            return Db.ExecuteAsync(query, new { messageId, userId });
        }
        public Task<int> RecipientsCountAsync(Guid id)
        {
            var query = "select COUNT(*) from dbo.\"ChatMessageRecipients\" where \"Id\"=@id";
            return Db.QueryFirstAsync<int>(query, new { id });
        }
        public async Task<IList<ChatMessage>> GetAsync(Guid chatId, int range, int offset, string userId)
        {
            var query = "select msges.*, senders.\"Id\", senders.\"UserName\", senders.\"AvatarSrc\", msg_users.*, atts.* from dbo.\"ChatMessages\" " +
                "as msges inner join dbo.\"Users\" as senders on senders.\"Id\"=msges.\"SenderId\" " +
                "full outer join dbo.\"ChatMessageUsers\" as msg_users on msg_users.\"MessageId\"=msges.\"Id\" full outer join " +
                "dbo.\"ChatMessageAttachments\" as atts on atts.\"MessageId\"=msges.\"Id\" where msges.\"Id\"=any(" +
                "select msges1.\"Id\" from dbo.\"ChatMessages\" as msges1 left outer join dbo.\"ChatMessageUsers\" as msg_users1 on " +
                "msges1.\"Id\" = msg_users1.\"MessageId\" where msg_users1.\"UserId\"=@userId and msges1.\"ChatId\"=@chatId " +
                $"group by msges1.\"Id\" order by msges1.\"SendDate\" desc limit {range} offset {offset}) order by msges.\"SendDate\" asc;";
            var msgList = new List<ChatMessage>();
            await Db.QueryAsync<ChatMessage, User, ChatMessageUser, ChatMessageAttachment, ChatMessage>(query,
                (msg, sender, re, att) =>
                {
                    var existingMsg = msgList.FirstOrDefault(x => x.Id == msg.Id);
                    if (existingMsg == null)
                    {
                        msg.Sender = sender;
                        msg.Recipients = new List<ChatMessageUser>();
                        msg.Attachments = new List<ChatMessageAttachment>();
                        if (re != null)
                            msg.Recipients.Add(re);
                        if (att != null)
                            msg.Attachments.Add(att);
                        msgList.Add(msg);
                    }
                    else
                    {
                        if (re != null)
                        {
                            var existingRe = existingMsg.Recipients.FirstOrDefault(x => x.MessageId == re.MessageId && x.UserId == re.UserId);
                            if (existingRe == null)
                                existingMsg.Recipients.Add(re);
                        }
                        if (att != null)
                        {
                            var existingAtt = existingMsg.Attachments.FirstOrDefault(x => x.Id == att.Id);
                            if (existingAtt == null)
                                existingMsg.Attachments.Add(att);
                        }
                    }
                    return msg;
                }, new { userId, chatId }, splitOn: "Id,Id,MessageId,Id");
            return msgList;
        }
        /// <summary>
        /// Получаем словарь Id сообщения -> кол-во вложений
        /// </summary>
        /// <param name="messageIds">Список сообщений для получения</param>
        /// <returns></returns>
        public async Task<IDictionary<Guid, int>> GetAttachmentsCountAsync(IList<Guid> messageIds)
        {
            var query = "select \"MessageId\", count(\"Id\") as \"AttCount\" from dbo.\"ChatMessageAttachments\" where \"MessageId\"=any(@messageIds) " +
                "group by \"MessageId\";";
            var dict = new Dictionary<Guid, int>();
            var result = await Db.QueryAsync(query, new { messageIds });
            foreach(var item in result)
            {
                dict[(Guid)item.MessageId] = (int)item.AttCount;
            }

            return dict;
        }
        public async Task RemoveListAsync(Guid[] ids, string userId)
        {
            var msgUsers = DbContext.MessageUsers.Where(x => x.UserId == userId && ids.Contains(x.MessageId));
            DbContext.MessageUsers.RemoveRange(msgUsers);
            await SaveAsync();

            var messages = await Table.Where(x => ids.Contains(x.Id)).Include(x => x.Recipients).ToListAsync();
            foreach(var msg in messages)
            {
                if (msg.Recipients.Count == 0)
                    Table.Remove(msg);
            }
            await SaveAsync();
        }
        public Task<ChatMessage> GetWithAttachmentsAsync(Guid messageId)
        {
            return Table.Include(x => x.Attachments).Include(x => x.Recipients).FirstOrDefaultAsync(x => x.Id == messageId);
        }
    }
}
