using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using Mite.CodeData.Enums;
using Mite.DAL.DTO;
using Dapper;

namespace Mite.DAL.Repositories
{
    public class FavoritePostsRepository : Repository<FavoritePost>
    {
        public FavoritePostsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<bool> IsFavoriteAsync(Guid postId, string userId)
        {
            return await Table.CountAsync(x => x.PostId == postId & x.UserId == userId) > 0;
        }
        /// <summary>
        /// Сколько польз. добавили в избранное
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<int> FavoriteCountAsync(Guid postId)
        {
            return await Table.CountAsync(x => x.PostId == postId);
        }
    }
}
