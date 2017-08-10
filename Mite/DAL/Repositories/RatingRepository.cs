using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using System;
using System.Collections.Generic;
using Mite.DAL.Infrastructure;
using System.Linq;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public sealed class RatingRepository : Repository<Rating>
    {
        public RatingRepository(AppDbContext db) : base(db)
        {
        }
        /// <summary>
        /// Возвращает рейтинг по пользователю и посту
        /// </summary>
        /// <param name="postId">что оценили</param>
        /// <param name="userId">кто оценил</param>
        /// <returns></returns>
        public async Task<Rating> GetByUserAndPostAsync(Guid postId, string userId)
        {
            var rating = await Table.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);
            return rating;
        }
        public async Task<Rating> GetByUserAndCommentAsync(string commentId, string userId)
        {
            var rating = await Table.FirstOrDefaultAsync(x => x.CommentId == Guid.Parse(commentId) && x.UserId == userId);
            return rating;
        }
        /// <summary>
        /// Возвращает рейтинги комментариев определенного пользователя
        /// </summary>
        /// <param name="commentsIds">Id комментариев, среди которых ищем рейтинг</param>
        /// <param name="userId">Пользователь, который их оценивал</param>
        /// <returns></returns>
        public async Task<IEnumerable<Rating>> GetCommentsRatings(IEnumerable<Guid> commentsIds, string userId)
        {
            var ratings = await Table.Where(x => x.UserId == userId && commentsIds.Any(y => y == x.Id)).ToListAsync();
            return ratings;
        }
        public override async Task AddAsync(Rating entity)
        {
            
            if(string.IsNullOrWhiteSpace(entity.UserId) || string.IsNullOrWhiteSpace(entity.OwnerId))
            {
                throw new NullReferenceException("Пустые Id пользователей недопустимы");
            }
            var query = "insert into dbo.\"Ratings\" (\"Value\", \"CommentId\", \"PostId\", \"UserId\", \"OwnerId\", \"RateDate\") " +
                "values(@Value, @CommentId, @PostId, @UserId, @OwnerId, @RateDate); ";
            if (entity.PostId != Guid.Empty && entity.PostId != null)
            {
                query += "with \"RatingPostSum\" as (select SUM(\"Value\") from dbo.\"Ratings\" where \"PostId\"=@PostId) " +
                    "update dbo.\"Posts\" set \"Rating\" = \"RatingPostSum\" where \"Id\"=@PostId; ";
            }
            else if(entity.CommentId != Guid.Empty && entity.CommentId != null)
            {
                query += "with \"RatingCommentsSum\" as (select SUM(\"Value\") from dbo.\"Ratings\" where \"CommentId\"=@CommentId) " +
                    "update dbo.\"Comments\" set \"Rating\"=\"RatingCommentsSum\" where \"Id\"=@CommentId; ";
            }
            else
            {
                throw new NullReferenceException("Id комментария и Id поста не могут быть пустыми одновременно");
            }
            query += "update dbo.\"Users\" set \"Rating\" = (select SUM(\"Value\") from dbo.\"Ratings\" where \"OwnerId\"=@OwnerId) where \"Id\"=@OwnerId; ";

            await Db.ExecuteAsync(query, entity);
        }
        public override async Task UpdateAsync(Rating entity)
        {
            //Запрос для обновления рейтинга поста или комментария, и пользователя
            var query = "update dbo.\"Ratings\" set\"Value\"= @Value where \"Id\"=@Id; ";

            if (entity.PostId != Guid.Empty && entity.PostId != null)
            {
                query += "with \"RatingPostSum\" as (select SUM(\"Value\") from dbo.\"Ratings\" where \"PostId\"=@PostId) " +
                    "update dbo.\"Posts\" set \"Rating\" = \"RatingPostSum\" where \"Id\"=@PostId; ";
            }
            else if (entity.CommentId != Guid.Empty && entity.CommentId != null)
            {
                query += "with \"RatingCommentsSum\" as (select SUM(\"Value\") from dbo.\"Ratings\" where \"CommentId\"=@CommentId) " +
                    "update dbo.\"Comments\" set \"Rating\" = \"RatingCommentsSum\" where \"Id\"=@CommentId; ";
            }
            else
            {
                throw new NullReferenceException("Id комментария и Id поста не могут быть пустыми одновременно");
            }
            query += "update dbo.\"Users\" set \"Rating\" = (select SUM(\"Value\") from dbo.\"Ratings\" where \"OwnerId\"=@OwnerId) where \"Id\"=@OwnerId; ";

            await Db.ExecuteAsync(query, entity);
        }
        /// <summary>
        /// Возвращаем рейтинги по оценившему пользователю(асинхронно) 
        /// </summary>
        /// <param name="ratedUser">Оценщик</param>
        /// <returns></returns>
        public async Task<IEnumerable<Rating>> GetAsync(string ratedUser)
        {
            return await Table.Where(x => x.UserId == ratedUser).ToListAsync();
        }
        /// <summary>
        /// Возвращаем рейтинги по оценившему пользователю
        /// </summary>
        /// <param name="ratedUser">Оценщик</param>
        /// <returns></returns>
        public IEnumerable<Rating> Get(string ratedUser)
        {
            return Table.Where(x => x.UserId == ratedUser).ToList();
        }
    }
}