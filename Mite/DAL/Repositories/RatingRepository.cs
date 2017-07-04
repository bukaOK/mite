using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using System;
using System.Collections.Generic;

namespace Mite.DAL.Repositories
{
    public sealed class RatingRepository : Repository<Rating>
    {
        public RatingRepository(IDbConnection db) : base(db)
        {
        }
        /// <summary>
        /// Возвращает рейтинг по пользователю и посту
        /// </summary>
        /// <param name="postId">что оценили</param>
        /// <param name="userId">кто оценил</param>
        /// <returns></returns>
        public Task<Rating> GetByUserAndPostAsync(string postId, string userId)
        {
            var param = new { UserId = userId, PostId = postId };
            return Db.QueryFirstOrDefaultAsync<Rating>("select * from dbo.Ratings where UserId=@UserId and PostId=@PostId", param);
        }
        public Task<Rating> GetByUserAndCommentAsync(string commentId, string userId)
        {
            var param = new { UserId = userId, CommentId = commentId };
            return Db.QueryFirstOrDefaultAsync<Rating>("select * from dbo.Ratings where UserId=@UserId and CommentId=@CommentId", param);
        }
        /// <summary>
        /// Возвращает рейтинги комментариев определенного пользователя
        /// </summary>
        /// <param name="commentsIds">Id комментариев, среди которых ищем рейтинг</param>
        /// <param name="userId">Пользователь, который их оценивал</param>
        /// <returns></returns>
        public Task<IEnumerable<Rating>> GetCommentsRatings(IEnumerable<Guid> commentsIds, string userId)
        {
            var query = "select * from dbo.Ratings where CommentId in @Ids and UserId=@UserId";
            return Db.QueryAsync<Rating>(query, new { Ids = commentsIds, UserId = userId });
        }
        public override async Task AddAsync(Rating entity)
        {
            
            if(string.IsNullOrWhiteSpace(entity.UserId) || string.IsNullOrWhiteSpace(entity.OwnerId))
            {
                throw new Exception("Пустые Id пользователей недопустимы");
            }
            var query = "insert into dbo.Ratings (Value, CommentId, PostId, UserId, OwnerId, RateDate) " +
                "values(@Value, @CommentId, @PostId, @UserId, @OwnerId, @RateDate); ";
            if (entity.PostId != Guid.Empty && entity.PostId != null)
            {
                query += "update dbo.Posts set Rating = (select SUM(Value) from dbo.Ratings where PostId=@PostId) where Id=@PostId; ";
            }
            else if(entity.CommentId != Guid.Empty && entity.CommentId != null)
            {
                query += "update dbo.Comments set Rating = (select SUM(Value) from dbo.Ratings where CommentId=@CommentId) where Id=@CommentId; ";
            }
            else
            {
                throw new NullReferenceException("Id комментария и Id поста не могут быть пустыми одновременно");
            }
            query += "update dbo.AspNetUsers set Rating = (select SUM(Value) from dbo.Ratings where OwnerId=@OwnerId) where Id=@OwnerId; ";

            await Db.ExecuteAsync(query, entity);
        }
        public override async Task UpdateAsync(Rating entity)
        {
            //Запрос для обновления рейтинга поста или комментария, и пользователя
            var query = "update dbo.Ratings set Value = @Value where Id=@Id; ";

            if (entity.PostId != Guid.Empty && entity.PostId != null)
            {
                query += "update dbo.Posts set Rating = (select SUM(Value) from dbo.Ratings where PostId=@PostId) where Id=@PostId; ";
            }
            else if (entity.CommentId != Guid.Empty && entity.CommentId != null)
            {
                query += "update dbo.Comments set Rating = (select SUM(Value) from dbo.Ratings where CommentId=@CommentId) where Id=@CommentId; ";
            }
            else
            {
                throw new NullReferenceException("Id комментария и Id поста не могут быть пустыми одновременно");
            }
            query += "update dbo.AspNetUsers set Rating = (select SUM(Value) from dbo.Ratings where OwnerId=@OwnerId) where Id=@OwnerId; ";

            await Db.ExecuteAsync(query, entity);
        }
        /// <summary>
        /// Возвращаем рейтинги по оценившему пользователю(асинхронно) 
        /// </summary>
        /// <param name="ratedUser">Оценщик</param>
        /// <returns></returns>
        public Task<IEnumerable<Rating>> GetAsync(string ratedUser)
        {
            var query = "select * from dbo.Ratings where UserId = @ratedUser";
            return Db.QueryAsync<Rating>(query, new { ratedUser });
        }
        /// <summary>
        /// Возвращаем рейтинги по оценившему пользователю
        /// </summary>
        /// <param name="ratedUser">Оценщик</param>
        /// <returns></returns>
        public IEnumerable<Rating> Get(string ratedUser)
        {
            var query = "select * from dbo.Ratings where UserId = @ratedUser";
            return Db.Query<Rating>(query, new { ratedUser });
        }
    }
}