using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using AutoMapper;
using Mite.DAL.Repositories;

namespace Mite.BLL.Services
{
    public interface IRatingService
    {
        /// <summary>
        /// Возвращает оценку пользователя к посту
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<PostRatingModel> GetByPostAndUserAsync(Guid postId, string userId);
        /// <summary>
        /// Возвращает оценку пользователя к комменту
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<CommentRatingModel> GetByCommentAndUserAsync(Guid commentId, string userId);
        Task RatePostAsync(PostRatingModel model);
        Task RateCommentAsync(CommentRatingModel model);
    }
    public class RatingService : DataService, IRatingService
    {
        public RatingService(IUnitOfWork database) : base(database)
        {
        }

        public async Task<CommentRatingModel> GetByCommentAndUserAsync(Guid commentId, string userId)
        {
            var repo = Database.GetRepo<RatingRepository, Rating>();

            var rating = await repo.GetByUserAndCommentAsync(commentId.ToString(), userId);
            if (rating == default(Rating))
                return null;

            return Mapper.Map<CommentRatingModel>(rating);
        }

        public async Task<PostRatingModel> GetByPostAndUserAsync(Guid postId, string userId)
        {
            var repo = Database.GetRepo<RatingRepository, Rating>();
            var rating = await repo.GetByUserAndPostAsync(postId.ToString(), userId);
            if (rating == default(Rating))
                return null;

            return Mapper.Map<PostRatingModel>(rating);
        }

        public async Task RateCommentAsync(CommentRatingModel model)
        {
            var repo = Database.GetRepo<RatingRepository, Rating>();
            var rating = Mapper.Map<Rating>(model);
            rating.RateDate = DateTime.UtcNow;

            if (rating.Id != Guid.Empty)
            {
                await repo.UpdateAsync(rating);
            }
            else
            {
                var existingRating =
                    await repo.GetByUserAndCommentAsync(rating.CommentId.ToString(), rating.UserId);

                //Если существует, обновляем рейтинг, иначе добавляем новый
                if (existingRating != default(Rating))
                {
                    rating.Id = existingRating.Id;
                    rating.OwnerId = existingRating.OwnerId;
                    await repo.UpdateAsync(rating);
                }
                else
                {
                    var comment = await Database.GetRepo<CommentsRepository, Comment>().GetAsync((Guid)rating.CommentId);
                    rating.OwnerId = comment.UserId;
                    await repo.AddAsync(rating);
                }
            }
        }

        public async Task RatePostAsync(PostRatingModel ratingModel)
        {
            var rating = Mapper.Map<Rating>(ratingModel);
            rating.RateDate = DateTime.UtcNow;
            var repo = Database.GetRepo<RatingRepository, Rating>();

            if (rating.Id != Guid.Empty)
            {
                var existingRating = await repo.GetAsync(rating.Id);
                rating.OwnerId = existingRating.OwnerId;
                await repo.UpdateAsync(rating);
            }
            else
            {
                var existingRating =
                    await repo.GetByUserAndPostAsync(rating.PostId.ToString(), rating.UserId);

                //Если существует, обновляем рейтинг, иначе добавляем новый
                if (existingRating != default(Rating))
                {
                    rating.Id = existingRating.Id;
                    rating.OwnerId = existingRating.OwnerId;
                    await repo.UpdateAsync(rating);
                }
                else
                {
                    var post = await Database.GetRepo<PostsRepository, Post>().GetAsync((Guid)rating.PostId);
                    rating.OwnerId = post.UserId;
                    await repo.AddAsync(rating);
                }
                    
            }
        }
    }
}