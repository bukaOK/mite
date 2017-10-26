using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Infrastructure;
using System.Linq;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class NotificationRepository : Repository<Notification>
    {
        public NotificationRepository(AppDbContext db) : base(db)
        {
        }
        public async Task<IEnumerable<Notification>> GetByUserAsync(string userId, bool onlyNew)
        {
            var notifications = await Table
                .Where(x => x.UserId == userId && (onlyNew ? x.IsNew : x.IsNew || !x.IsNew))
                .Include(x => x.NotifyUser)
                .ToListAsync();
            return notifications;
        }
        public Task ReadByUserAsync(string userId)
        {
            var query = "update dbo.\"Notifications\" set \"IsNew\"=false where \"UserId\"=@UserId and \"IsNew\"=true;";
            return Db.ExecuteAsync(query, new { UserId = userId });
        }
        public Task RemoveByUserAsync(string userId)
        {
            var query = "delete from dbo.\"Notifications\" where \"UserId\"=@userId;";
            return Db.ExecuteAsync(query, new { userId });
        }
    }
}