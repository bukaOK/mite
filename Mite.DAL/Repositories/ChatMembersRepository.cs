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
using Mite.CodeData.Enums;

namespace Mite.DAL.Repositories
{
    public class ChatMembersRepository : Repository<ChatMember>
    {
        public ChatMembersRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<IEnumerable<ChatMember>> GetByChatAsync(Guid chatId)
        {
            var members = await Table.AsNoTracking().Include(x => x.User).Include(x => x.Inviter)
                .Where(x => x.ChatId == chatId && x.Status == ChatMemberStatuses.InChat)
                .OrderByDescending(x => x.EnterDate).ThenBy(x => x.UserId).ToListAsync();
            return members;
        }
        public async Task AddListAsync(IEnumerable<ChatMember> members)
        {
            Table.AddRange(members);
            await SaveAsync();
        }
        public async Task<bool> IsMemberAsync(Guid chatId, string userId)
        {
            var member = await Table.FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == userId);
            return member != null;
        }
    }
}
