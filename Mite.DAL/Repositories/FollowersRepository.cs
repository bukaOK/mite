using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;
using Mite.CodeData.Enums;
using System;

namespace Mite.DAL.Repositories
{
    public sealed class FollowersRepository : Repository<Follower>
    {
        public FollowersRepository(AppDbContext db) : base(db)
        {
        }

        public Task<IEnumerable<Follower>> GetFollowersByUserAsync(string userId, SortFilter sort)
        {
            var query = "select * from dbo.\"Followers\" as followers inner join dbo.\"Users\" as users " +
                "on followers.\"UserId\"=users.\"Id\" where followers.\"FollowingUserId\"=@userId order by ";
            switch (sort)
            {
                case SortFilter.New:
                    query += "followers.\"FollowTime\" desc";
                    break;
                case SortFilter.Old:
                    query += "followers.\"FollowTime\" asc";
                    break;
                case SortFilter.Popular:
                    query += "users.\"Rating\" desc, followers.\"FollowTime\" desc";
                    break;
            }
            query += ";";
            return Db.QueryAsync<Follower, User, Follower>(query, (follower, user) =>
            {
                follower.User = user;
                return follower;
            }, new { userId });
        }
        public async Task<bool> IsFollowerAsync(string followerId, string followingId)
        {
            return await Table.AnyAsync(x => x.UserId == followerId && x.FollowingUserId == followingId);
        }
        public async Task RemoveAsync(string followerId, string followingId)
        {
            var follower = await Table.FirstOrDefaultAsync(x => x.UserId == followerId && x.FollowingUserId == followingId);
            Table.Remove(follower);
            await SaveAsync();
        }
        public async Task<IEnumerable<Follower>> GetFollowingsByUserAsync(string userId, SortFilter sort)
        {
            var folQuery = Table.AsNoTracking().Where(x => x.UserId == userId).Include(x => x.FollowingUser);
            switch (sort)
            {
                case SortFilter.New:
                    folQuery = folQuery.OrderByDescending(x => x.FollowingUser.RegisterDate)
                        .ThenByDescending(x => x.FollowingUser.Rating);
                    break;
                case SortFilter.Old:
                    folQuery = folQuery.OrderBy(x => x.FollowingUser.RegisterDate)
                        .ThenByDescending(x => x.FollowingUser.Rating);
                    break;
                case SortFilter.Popular:
                    folQuery = folQuery.OrderByDescending(x => x.FollowingUser.Rating)
                        .ThenBy(x => x.FollowingUser.RegisterDate);
                    break;
            }
            var followings = await folQuery.ToListAsync();
            return followings;
        }
        public async Task<int> GetFollowersCount(string followingId)
        {
            return await Table.CountAsync(x => x.FollowingUserId == followingId);
        }
        public Task<int> GetFollowingsCountAsync(string followerId)
        {
            return Table.CountAsync(x => x.UserId == followerId);
        }
        /// <summary>
        /// Подписчики пользователя для добавления в чат
        /// </summary>
        /// <param name="chatId">Чат</param>
        /// <param name="userId">Пользователь, для которого ищем подписчиков</param>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetForChatAsync(Guid chatId, string userId)
        {
            var query = "select users.* from dbo.\"Followers\" as followers inner join dbo.\"Users\" as users on users.\"Id\"=followers.\"UserId\" " +
                "left outer join (select ch_mem.\"Status\", ch_mem.\"UserId\" from dbo.\"ChatMembers\" as ch_mem where ch_mem.\"ChatId\"=@chatId) " +
                "as chat_members on chat_members.\"UserId\"=users.\"Id\" where followers.\"FollowingUserId\"=@userId and " +
                "(chat_members.\"Status\" is null or chat_members.\"Status\"=@excludeStatus);";
            return await Db.QueryAsync<User>(query, new { chatId, userId, excludeStatus = ChatMemberStatuses.Excluded });
        }
    }
}