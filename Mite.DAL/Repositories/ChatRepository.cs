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

namespace Mite.DAL.Repositories
{
    public class ChatRepository : Repository<Chat>
    {
        public ChatRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public override async Task AddAsync(Chat entity)
        {
            var query = "insert into dbo.\"Chats\" values(@Id);";
            await Db.ExecuteAsync(query, entity);

            query = "insert into dbo.\"ChatMembers\" (\"ChatId\", \"UserId\") values(@Id, @UserId);";
            await Db.ExecuteAsync(query, entity.Members.Select(x => new
            {
                Id = entity.Id,
                UserId = x.Id
            }));
        }
        public Task<Chat> GetByMembersAsync(IEnumerable<string> userIds)
        {
            return Table.FirstOrDefaultAsync(x => x.Members.All(y => userIds.Contains(y.Id)));
        }
        public Task<Chat> GetWithMembersAsync(Guid id)
        {
            return Table.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id);
        }
        public Chat GetWithMembers(Guid id)
        {
            return Table.Include(x => x.Members).FirstOrDefault(x => x.Id == id);
        }
        public Task RemoveMemberAsync(Guid id, string userId)
        {
            var query = "delete from dbo.\"DialogMembers\" where Id=@id and UserId=@userId";
            return Db.ExecuteAsync(query, new { id, userId });
        }
    }
}
