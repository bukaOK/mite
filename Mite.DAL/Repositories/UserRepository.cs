using Dapper;
using Mite.BLL.DTO;
using Mite.CodeData.Enums;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.DAL.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(AppDbContext db) : base(db)
        {
        }
        /// <summary>
        /// Получить пользователей, у которых показывается реклама
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<UserAdDTO> GetAdUsers()
        {
            var query = "select \"Id\", \"Rating\", \"RatingsActivity\".\"RatingActivity\", \"CommentsActivity\".\"CommentActivity\" from " +
                "dbo.\"Users\" left outer join (select \"UserId\", COUNT(\"UserId\") as \"RatingActivity\" from dbo.\"Ratings\" group by \"UserId\") as " +
                "\"RatingsActivity\" on dbo.\"Users\".\"Id\"=\"RatingsActivity\".\"UserId\" left outer join " +
                "(select \"UserId\" as \"CommentUserId\", COUNT(\"UserId\") as \"CommentActivity\" from dbo.\"Comments\" group by \"UserId\") as \"CommentsActivity\" " +
                "on dbo.\"Users\".\"Id\"=\"CommentsActivity\".\"CommentUserId\" where not (\"RatingActivity\" is null and \"CommentActivity\" is null) and \"ShowAd\"=true;";
            return Db.Query<UserAdDTO>(query);
        }
        /// <summary>
        /// Получить надежность пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <param name="badCoef">На сколько умножаем "плохие" статусы</param>
        /// <param name="goodCoef">На сколько умножаем "хорошие" для надежности статусы</param>
        /// <returns></returns>
        public async Task RecountReliabilityAsync(string userId, int goodCoef, int badCoef)
        {
            var query = "select count(*) from dbo.\"Deals\" where (\"AuthorId\"=@userId or \"ClientId\"=@userId) and " +
                $"\"Status\"={(int)DealStatuses.Confirmed};";
            var qParams = new { userId };
            var goodCount = await Db.QueryFirstAsync<int>(query, qParams);
            query = "select count(*) from dbo.\"Deals\" where (\"AuthorId\"=@userId or \"ClientId\"=@userId) " +
                $"and (\"Status\"={(int)DealStatuses.ModerConfirmed} or \"Status\"={(int)DealStatuses.ModerRejected});";
            var badCount = await Db.QueryFirstAsync<int>(query, qParams);
            var reliability = badCount * badCoef + goodCount * goodCoef;
            var user = await Table.FirstAsync(x => x.Id == userId);
            if (user.Reliability != reliability)
            {
                user.Reliability = reliability;
                DbContext.Entry(user).Property(x => x.Reliability).IsModified = true;
                await SaveAsync();
            }
        }
    }
}