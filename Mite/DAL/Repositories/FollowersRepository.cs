using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;

namespace Mite.DAL.Repositories
{
    public sealed class FollowersRepository : Repository<Follower>
    {
        public FollowersRepository(AppDbContext db) : base(db)
        {
        }

        public async Task<IEnumerable<Follower>> GetFollowersByUserAsync(string userId)
        {
            var followers = await Table.Where(x => x.FollowingUserId == userId).Include(x => x.User).ToListAsync();
            return followers;
        }
        public async Task<bool> IsFollower(string followerId, string followingId)
        {
            var count = await Table.CountAsync(x => x.UserId == followerId && x.FollowingUserId == followingId);
            return count > 0;
        }
        public async Task RemoveAsync(string followerId, string followingId)
        {
            var follower = await Table.FirstOrDefaultAsync(x => x.UserId == followerId && x.FollowingUserId == followingId);
            Table.Remove(follower);
            await SaveAsync();
        }
        public async Task<IEnumerable<Follower>> GetFollowingsByUserAsync(string userId)
        {
            var followings = await Table.Where(x => x.UserId == userId).Include(x => x.FollowingUser).ToListAsync();
            return followings;
        }
        public async Task<int> GetFollowersCount(string followingId)
        {
            return await Table.CountAsync(x => x.FollowingUserId == followingId);
        }
    }
}