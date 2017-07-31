using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Infrastructure;

namespace Mite.DAL.Repositories
{
    public class NotificationRepository : Repository<Notification>
    {
        public NotificationRepository(AppDbContext db) : base(db)
        {
        }
        public Task<IEnumerable<Notification>> GetByUserAsync(string userId, bool onlyNew)
        {
            var query = "select * from dbo.Notifications inner join dbo.AspNetUsers on NotifyUserId=dbo.AspNetUsers.Id "
                + "where UserId=@userId";
            if (onlyNew)
                query += " and IsNew=1";
            return Db.QueryAsync<Notification, User, Notification>(query, (notification, user) =>
            {
                notification.NotifyUser = user;
                return notification;
            }, new { userId });
        }
        public Task ReadByUserAsync(string userId)
        {
            var query = "update dbo.Notifications set IsNew=0 where UserId=@UserId and IsNew=1";
            return Db.ExecuteAsync(query, new { UserId = userId });
        }
        public Task RemoveByUserAsync(string userId)
        {
            var query = "delete from dbo.Notifications where UserId=@userId";
            return Db.ExecuteAsync(query, new { userId });
        }
    }
}