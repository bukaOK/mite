﻿using Dapper;
using Mite.BLL.DTO;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

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
        public int Count(string roleName)
        {
            var query = "select count(*) from dbo.\"Users\" users inner join dbo.\"UserRoles\" user_roles on " +
                "user_roles.\"UserId\"=users.\"Id\" inner join dbo.\"Roles\" roles on roles.\"Id\"=user_roles.\"RoleId\" " +
                "where roles.\"Name\"=@roleName;";
            return Db.QueryFirst<int>(query, new { roleName });
        }
        /// <summary>
        /// Огр. кол-во пользователей(для лендинга)
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<User> RandomUsers(int count)
        {
            var query = $"select * from dbo.\"Users\" where \"AvatarSrc\" is not null and \"AvatarSrc\"!='{PathConstants.AvatarSrc}' " +
                $"order by random() limit {count};";
            return Db.Query<User>(query);
        }
        /// <summary>
        /// Подписаться на тег
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task AddTagAsync(Guid tagId, string userId)
        {
            var query = "insert into dbo.\"UserTags\" (\"TagId\", \"UserId\") values(@tagId, @userId);";
            return Db.ExecuteAsync(query, new { tagId, userId });
        }
        public Task RemoveTagAsync(Guid tagId, string userId)
        {
            var query = "delete from dbo.\"UserTags\" where \"TagId\"=@tagId and \"UserId\"=@userId;";
            return Db.ExecuteAsync(query, new { tagId, userId });
        }
        /// <summary>
        /// Подписан ли на какие нибудь теги
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool HasAnyTags(string userId)
        {
            var query = "select count(*) from dbo.\"UserTags\" where \"UserId\"=@userId;";
            var count = Db.QueryFirst<int>(query, new { userId });
            return count > 0;
        }
        /// <summary>
        /// Получить лучших авторов за промежуток времени
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Task<IEnumerable<User>> GetDateBestAsync(DateTime beginDate, DateTime endDate, int count)
        {
            var query = "select * from dbo.\"Users\" users where users.\"Id\" in (select posts.\"UserId\" from dbo.\"Posts\" posts left outer join " +
                "(select \"PostId\", sum(ratings.\"Value\") as \"RateValue\" from dbo.\"Ratings\" ratings where \"RateDate\" <= @endDate and " +
                "\"RateDate\" >= @beginDate and \"PostId\" is not null group by \"PostId\") as rate_vals on rate_vals.\"PostId\"=posts.\"Id\" " +
                "group by posts.\"UserId\" having sum(rate_vals.\"RateValue\") is not null order by sum(rate_vals.\"RateValue\") desc limit @count);";
            return Db.QueryAsync<User>(query, new { beginDate, endDate, count });
        }
    }
}