using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Mite.DAL.DTO;
using Dapper;
using System.Data.Entity;
using System.Data;
using Mite.CodeData.Enums;

namespace Mite.DAL.Repositories
{
    public class ChatMessagesRepository : Repository<ChatMessage>
    {
        public ChatMessagesRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task ReadAsync(Guid messageId, string userId)
        {
            if (Db.State != ConnectionState.Closed)
                return;
            var msgUser = await DbContext.MessageUsers.FirstOrDefaultAsync(x => x.MessageId == messageId && x.UserId == userId);
            msgUser.Read = true;
            DbContext.Entry(msgUser).Property(x => x.Read).IsModified = true;
            await SaveAsync();
        }
        public void Read(Guid messageId, string userId)
        {
            if (Db.State != ConnectionState.Closed)
                return;
            var msgUser = DbContext.MessageUsers.FirstOrDefault(x => x.MessageId == messageId && x.UserId == userId);
            msgUser.Read = true;
            DbContext.Entry(msgUser).Property(x => x.Read).IsModified = true;
            Save();
        }
        public void ReadUnreaded(Guid chatId, string userId)
        {
            if (Db.State != ConnectionState.Closed)
                return;
            var msgUsers = DbContext.MessageUsers.Where(x => x.Message.ChatId == chatId && x.UserId == userId).ToList();
            if (msgUsers.Count == 0)
                return;
            foreach(var msgUser in msgUsers)
            {
                msgUser.Read = true;
                DbContext.Entry(msgUser).Property(x => x.Read).IsModified = true;
            }
            Save();
        }
        public async Task RemoveAsync(Guid messageId, string userId)
        {
            var msgUser = await DbContext.MessageUsers.FirstOrDefaultAsync(x => x.MessageId == messageId && x.UserId == userId);
            DbContext.MessageUsers.Remove(msgUser);
            await SaveAsync();
        }
        public Task<int> RecipientsCountAsync(Guid id)
        {
            var query = "select COUNT(*) from dbo.\"ChatMessageRecipients\" where \"Id\"=@id";
            return Db.QueryFirstAsync<int>(query, new { id });
        }
        public async Task<IList<ChatMessage>> GetAsync(Guid chatId, int range, int offset, string userId)
        {
            var chat = await DbContext.Chats.FirstAsync(x => x.Id == chatId);
            var isMember = await DbContext.ChatMembers.AnyAsync(x => x.UserId == userId);

            var query = "select msges.*, sender.\"Id\", sender.\"UserName\", sender.\"AvatarSrc\", msg_users.*, atts.* from dbo.\"ChatMessages\" " +
                "as msges inner join dbo.\"Users\" as sender on sender.\"Id\"=msges.\"SenderId\" " +
                "full outer join dbo.\"ChatMessageUsers\" as msg_users on msg_users.\"MessageId\"=msges.\"Id\" full outer join " +
                "dbo.\"ChatMessageAttachments\" as atts on atts.\"MessageId\"=msges.\"Id\" where msges.\"Id\" in (" +
                "select msges1.\"Id\" from dbo.\"ChatMessages\" msges1 inner join dbo.\"ChatMessageUsers\" msg_users1 on ";
            if (chat.Type != ChatTypes.Public || isMember)
                query += "msges1.\"Id\"=msg_users1.\"MessageId\" where msg_users1.\"UserId\"=@userId and msges1.\"ChatId\"=@chatId ";
            else
                query += "msges1.\"Id\"=msg_users1.\"MessageId\" where msges1.\"ChatId\"=@chatId ";

            query += $"group by msges1.\"Id\" order by msges1.\"SendDate\" desc limit {range} offset {offset}" +
                ") order by msges.\"SendDate\" asc;";

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
        /// Кол-во новых сообщений
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<int> GetNewCountAsync(string userId)
        {
            var count = await Table.CountAsync(x => x.Recipients.Any(y => y.UserId == userId && !y.Read) 
                && x.Chat.Type != ChatTypes.Deal && x.Chat.Type != ChatTypes.Dispute);
            return count;
        }
        public Task<IEnumerable<NewMessagesCountDTO>> GetNewCountAsync(string userId, List<Guid> chatIds)
        {
            var query = "select msgs.\"ChatId\", count(msgs.\"ChatId\") as \"MessagesCount\" from dbo.\"ChatMessages\" msgs " +
                "left outer join dbo.\"ChatMessageUsers\" msg_users on msg_users.\"MessageId\"=msgs.\"Id\" " +
                "where msg_users.\"UserId\"=@userId and msg_users.\"Read\"=false and msgs.\"ChatId\"=any(@chatIds) group by msgs.\"ChatId\";";
            return Db.QueryAsync<NewMessagesCountDTO>(query, new { userId, chatIds });
        }
        public int GetNewCount(string userId)
        {
            var count = Table.Count(x => x.Recipients.Any(y => y.UserId == userId && !y.Read) 
                && x.Chat.Type != ChatTypes.Deal && x.Chat.Type != ChatTypes.Dispute);
            return count;
        }
        public async Task<IList<ChatMessageAttachment>> RemoveListAsync(List<Guid> ids, string userId)
        {
            var qParams = new { userId, ids };
            var query = "delete from dbo.\"ChatMessageUsers\" where \"UserId\"=@userId and \"MessageId\"=any(@ids);";
            await Db.ExecuteAsync(query, qParams);
            query = "select msgs.\"Id\" from dbo.\"ChatMessages\" msgs left outer join dbo.\"ChatMessageUsers\" msg_users " +
                "on msgs.\"Id\"=msg_users.\"MessageId\" where msgs.\"Id\"=any(@ids) and msg_users.\"UserId\"=@userId " +
                "group by msgs.\"Id\" having count(msgs.\"Id\")=0;";
            var msgsToDel = await Db.QueryAsync<Guid>(query, qParams);
            if (msgsToDel.Count() > 0)
            {
                var delIds = msgsToDel.ToList();
                query = "select * from dbo.\"ChatMessageAttachments\" where \"MessageId\"=any(@ids);";
                var attsToRemove = await Db.QueryAsync<ChatMessageAttachment>(query, new { ids });
                query = "delete from dbo.\"ChatMessages\" msgs where msgs.\"Id\"=any(@ids);";
                await Db.ExecuteAsync(query, new { ids = delIds });
                return attsToRemove.ToList();
            }
            return null;
        }
        public async Task<IEnumerable<Guid>> GetByChatAsync(Guid chatId)
        {
            var query = "select \"Id\" from dbo.\"ChatMessages\" where \"ChatId\"=@chatId;";
            return await Db.QueryAsync<Guid>(query, new { chatId });
        }
        public Task<ChatMessage> GetWithAttachmentsAsync(Guid messageId)
        {
            return Table.Include(x => x.Attachments).Include(x => x.Recipients).FirstOrDefaultAsync(x => x.Id == messageId);
        }
    }
}
