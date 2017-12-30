using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using Dapper;
using System.Collections;
using Mite.CodeData.Enums;
using Mite.DAL.DTO;
using Mite.DAL.Filters;

namespace Mite.DAL.Repositories
{
    public class ChatRepository : Repository<Chat>
    {
        public ChatRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public override async Task AddAsync(Chat entity)
        {
            Table.Add(entity);
            await SaveAsync();
        }
        public async Task<(string imgSrc, string compressedSrc)> RemoveAsync(Guid chatId, string userId)
        {
            var membersCount = await DbContext.ChatMembers.CountAsync(x => x.ChatId == chatId && x.Status != ChatMemberStatuses.Removed);
            var chat = await Table.FirstAsync(x => x.Id == chatId);
            (string imgSrc, string compressedSrc) tuple = (null, null);
            if(membersCount - 1 <= 0)
            {
                tuple.imgSrc = chat.ImageSrc;
                tuple.compressedSrc = chat.ImageSrcCompressed;
                Table.Remove(chat);
            }
            else
            {
                var removeMember = await DbContext.ChatMembers.FirstAsync(x => x.UserId == userId && x.ChatId == chatId);
                removeMember.Status = ChatMemberStatuses.Removed;
                DbContext.Entry(removeMember).Property(x => x.Status).IsModified = true;
            }
            await SaveAsync();
            return tuple;
        }
        public Task<Chat> GetByMembersAsync(IEnumerable<string> userIds)
        {
            return Table.FirstOrDefaultAsync(x => userIds.Count() == x.Members.Count && x.Members.All(y => userIds.Contains(y.UserId)));
        }
        public async Task<IEnumerable<Chat>> GetByUserAsync(string userId)
        {
            var query = "select * from (select distinct on(chats.\"Id\") chats.*, last_msg.*, companion.* " +
                "from dbo.\"Chats\" as chats left outer join (select msgs.*, sender.* from dbo.\"ChatMessages\" as msgs " +
                "left outer join dbo.\"ChatMessageUsers\" as msg_users on msg_users.\"MessageId\"=msgs.\"Id\" " +
                "left outer join dbo.\"Users\" as sender on msgs.\"SenderId\"=sender.\"Id\" " +
                "where msg_users.\"UserId\"=@UserId order by msgs.\"SendDate\" desc) as last_msg on last_msg.\"ChatId\"=chats.\"Id\" " +
                "inner join dbo.\"ChatMembers\" as chat_members on chat_members.\"ChatId\"=chats.\"Id\" left outer join " +
                "(select * from dbo.\"ChatMembers\" as ch_mem1 inner join dbo.\"Users\" as mem on mem.\"Id\"=ch_mem1.\"UserId\" " +
                "where ch_mem1.\"UserId\" != @userId) as companion on companion.\"ChatId\"=chats.\"Id\" " +
                "where chats.\"Type\"!=@DisputeType and chats.\"Type\"!=@DealType and chat_members.\"UserId\"=@UserId and " +
                "chat_members.\"Status\"!=@RemovedStatus order by chats.\"Id\", last_msg.\"SendDate\" desc) as tbl order by tbl.\"SendDate\" desc;";
            return await Db.QueryAsync<Chat, ChatMessage, User, User, Chat>(query, (chat, msg, sender, companion) =>
            {
                if(msg != null)
                {
                    msg.Sender = sender ?? null;
                    chat.Messages = new List<ChatMessage> { msg };
                }
                if (string.IsNullOrEmpty(chat.ImageSrc) && companion != null)
                    chat.ImageSrc = companion.AvatarSrc;
                if (string.IsNullOrEmpty(chat.Name))
                    chat.Name = companion?.UserName ?? "Мой чат";
                return chat;
            }, new { UserId = userId, DisputeType = ChatTypes.Dispute, DealType = ChatTypes.Deal, RemovedStatus = ChatMemberStatuses.Removed });
        }
        public Task<Chat> GetWithMembersAsync(Guid id)
        {
            return Table.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id);
        }
        public Chat GetWithMembers(Guid id)
        {
            return Table.Include(x => x.Members).FirstOrDefault(x => x.Id == id);
        }
        public async Task<bool> IsMemberAsync(Guid chatId, string userId)
        {
            return await DbContext.ChatMembers.AnyAsync(x => x.ChatId == chatId && x.UserId == userId);
        }
        /// <summary>
        /// Получить подписчиков, которые не состоят в чате(один на один) с пользователем
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetActualFollowersAsync(string userId)
        {
            var query = "select users.* from dbo.\"Followers\" followers inner join dbo.\"Users\" users on users.\"Id\"=followers.\"UserId\" " +
                "where followers.\"FollowingUserId\"=@userId and users.\"Id\" not in" +
                "(select ch_mem.\"UserId\" from dbo.\"ChatMembers\" ch_mem right outer join " +
                //Отберем чаты, в которых есть наш пользователь
                "(select ch.\"Id\" from dbo.\"Chats\" ch left outer join dbo.\"ChatMembers\" ch_mem1 on ch_mem1.\"ChatId\"=ch.\"Id\" " +
                "where ch_mem1.\"UserId\"=@userId and ch.\"Type\"=@privateChatType and ch_mem1.\"Status\"!=@removedStatus) " +
                "chats on chats.\"Id\"=ch_mem.\"ChatId\");";
            return await Db.QueryAsync<User>(query, new { userId, privateChatType = ChatTypes.Private, removedStatus = ChatMemberStatuses.Removed });
        }
    }
}
