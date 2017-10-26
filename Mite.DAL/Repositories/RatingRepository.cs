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
        public Task<Rating> GetByUserAndPostAsync(Guid postId, string userId)
        {
            return Table.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);
        }
        public Task<Rating> GetByUserAndCommentAsync(Guid? commentId, string userId)
        {
            if (commentId == null)
                return null;
            return Table.FirstOrDefaultAsync(x => x.CommentId == commentId && x.UserId == userId);
        }
        /// <summary>
        /// Возвращает рейтинги комментариев определенного пользователя
        /// </summary>
        /// <param name="commentsIds">Id комментариев, среди которых ищем рейтинг</param>
        /// <param name="userId">Пользователь, который их оценивал</param>
        /// <returns></returns>
        public async Task<IEnumerable<Rating>> GetCommentsRatings(IEnumerable<Guid> commentsIds, string userId)
        {
            var ratings = await Table.Where(x => x.UserId == userId && commentsIds.Any(y => y == x.CommentId)).ToListAsync();
            return ratings;
        }
        public override async Task AddAsync(Rating entity)
        {
            if(string.IsNullOrWhiteSpace(entity.UserId) || string.IsNullOrWhiteSpace(entity.OwnerId))
            {
                throw new NullReferenceException("Пустые Id пользователей недопустимы");
            }
            Table.Add(entity);
            if (entity.PostId != null)
            {
                var post = await DbContext.Posts.FirstAsync(x => x.Id == entity.PostId);
                post.Rating += entity.Value;
                DbContext.Entry(post).Property(x => x.Rating).IsModified = true;
            }
            else if(entity.CommentId != null)
            {
                var comment = await DbContext.Comments.FirstAsync(x => x.Id == entity.CommentId);
                comment.Rating += entity.Value;
                DbContext.Entry(comment).Property(x => x.Rating).IsModified = true;
            }
            else
            {
                throw new NullReferenceException("Id комментария и Id поста не могут быть пустыми одновременно");
            }
            var user = await DbContext.Users.FirstAsync(x => x.Id == entity.OwnerId);
            user.Rating = await Table.Where(x => x.OwnerId == user.Id).SumAsync(x => x.Value);
            DbContext.Entry(user).Property(x => x.Rating).IsModified = true;

            await SaveAsync();
        }
        public override async Task UpdateAsync(Rating entity)
        {
            //Запрос для обновления рейтинга поста или комментария, и пользователя
            DbContext.Entry(entity).Property(x => x.Value).IsModified = true;

            if (entity.PostId != null)
            {
                var post = await DbContext.Posts.FirstAsync(x => x.Id == entity.PostId);
                post.Rating = await Table.Where(x => x.PostId == post.Id).SumAsync(x => x.Value);
                DbContext.Entry(post).Property(x => x.Rating).IsModified = true;
            }
            else if (entity.CommentId != Guid.Empty && entity.CommentId != null)
            {
                var comment = await DbContext.Comments.FirstAsync(x => x.Id == entity.CommentId);
                comment.Rating = await Table.Where(x => x.CommentId == comment.Id).SumAsync(x => x.Value);
                DbContext.Entry(comment).Property(x => x.Rating).IsModified = true;
            }
            else if(entity.AuthorServiceId != null)
            {
                var authorService = await DbContext.AuthorServices.FirstAsync(x => x.Id == entity.AuthorServiceId);
                authorService.Rating = await Table.Where(x => x.AuthorServiceId == authorService.Id).SumAsync(x => x.Value);
                DbContext.Entry(authorService).Property(x => x.Rating).IsModified = true;
            }
            else
            {
                throw new NullReferenceException("Id комментария и Id поста не могут быть пустыми одновременно");
            }
            var user = await DbContext.Users.FirstAsync(x => x.Id == entity.OwnerId);
            user.Rating = await Table.Where(x => x.OwnerId == user.Id).SumAsync(x => x.Value);
            DbContext.Entry(user).Property(x => x.Rating).IsModified = true;

            await SaveAsync();
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