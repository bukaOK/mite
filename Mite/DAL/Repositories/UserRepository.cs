using Dapper;
using Mite.BLL.DTO;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            var query = "select [Id], [Rating], RatingsActivity.RatingActivity, CommentsActivity.CommentActivity from " +
                "dbo.AspNetUsers left outer join (select [UserId], COUNT(UserId) as RatingActivity from dbo.Ratings group by UserId) as " +
                "RatingsActivity on dbo.AspNetUsers.Id=RatingsActivity.UserId left outer join " +
                "(select UserId as [CommentUserId], COUNT(UserId) as CommentActivity from dbo.Comments group by UserId) as CommentsActivity " +
                "on dbo.AspNetUsers.Id=CommentsActivity.CommentUserId where not (RatingActivity is null and CommentActivity is null) and ShowAd=1";
            return Db.Query<UserAdDTO>(query);
        }
    }
}