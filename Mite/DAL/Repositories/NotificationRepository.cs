using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Mite.DAL.Repositories
{
    public class NotificationRepository : Repository<Notification>
    {
        public NotificationRepository(IDbConnection db) : base(db)
        {
        }
        public Task<IEnumerable<Notification>> GetNewNotificationsByUserAsync(string userId)
        {
            var query = "select * from dbo.Notifications inner join dbo.AspNetUsers on NotifyUserId=dbo.AspNetUsers.Id "
                + "where UserId=@userId and IsNew=1";
            return Db.QueryAsync<Notification, User, Notification>(query, (notification, user) =>
            {
                notification.NotifyUser = user;
                return notification;
            }, new { userId });
        }
        public Task<IEnumerable<Notification>> GetNotificationsByUserAsync(string userId)
        {
            var query = "select * from dbo.Notifications where UserId=@UserId";
            return Db.QueryAsync<Notification>(query, new { UserId = userId });
        }
        public Task ReadByUserAsync(string userId)
        {
            var query = "update dbo.Notifications set IsNew=0 where UserId=@UserId and IsNew=1";
            return Db.ExecuteAsync(query, new { UserId = userId });
        }
        public int GetNewNotificationsCount(string userId)
        {
            var query = "select COUNT(*) from dbo.Notifications where UserId=@userId and IsNew=1";
            return Db.QueryFirst<int>(query, new { userId });
        }
        public Task<int> GetNewNotificationsCountAsync(string userId)
        {
            var query = "select COUNT(*) from dbo.Notifications where UserId=@userId and IsNew=1";
            return Db.QueryFirstAsync<int>(query, new { userId });
        }
    }
}