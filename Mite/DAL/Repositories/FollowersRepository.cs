﻿using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public sealed class FollowersRepository
    {
        private IDbConnection Db;
        public FollowersRepository(IDbConnection db)
        {
            Db = db;
        }
        public async Task<IEnumerable<Follower>> GetFollowersByUserAsync(string userId)
        {
            var query = "select * from dbo.Followers inner join dbo.AspNetUsers on"
                + " dbo.AspNetUsers.Id=dbo.Followers.Id where FollowingUserId=@UserId";
            var followers = await Db.QueryAsync<Follower, User, Follower>(query, (follower, user) =>
            {
                follower.User = user;
                return follower;
            }, new { UserId = userId });
            return followers;
        }
        public Task AddAsync(Follower entity)
        {
            var query = "insert into dbo.Followers(Id, FollowingUserId, FollowTime)" +
                "values(@Id, @FollowingUserId, @FollowTime)";
            return Db.ExecuteAsync(query, entity);
        }
        public async Task<bool> IsFollower(string followerId, string followingId)
        {
            var query = "select COUNT(*) from dbo.Followers where Id=@Id and FollowingUserId=@FlId";
            var count = await Db.QueryFirstAsync<int>(query, new { Id = followerId, FlId = followingId });
            return count > 0;
        }
        public Task RemoveAsync(string followerId, string followingId)
        {
            var query = "delete from dbo.Followers where Id=@Id and FollowingUserId=@FollowingUserId";
            return Db.ExecuteAsync(query, new { Id = followerId, FollowingUserId = followingId });
        }
        public Task<IEnumerable<Follower>> GetFollowingsByUserAsync(string userId)
        {
            var query = "select * from dbo.Followers inner join dbo.AspNetUsers on " +
                "dbo.AspNetUsers.Id = dbo.Followers.FollowingUserId where dbo.Followers.Id=@userId";
            return Db.QueryAsync<Follower, User, Follower>(query, (follower, user) =>
            {
                follower.FollowingUser = user;
                return follower;
            }, new { userId });
        }
        public Task<int> GetFollowersCount(string followingId)
        {
            var query = "select COUNT(*) from dbo.Followers where FollowingUserId=@FollowingUserId";
            return Db.QueryFirstAsync<int>(query, new { FollowingUserId = followingId });
        }
    }
}