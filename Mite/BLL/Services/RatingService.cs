using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using AutoMapper;
using Mite.DAL.Repositories;
using NLog;
using Mite.CodeData.Enums;
using Mite.BLL.IdentityManagers;

namespace Mite.BLL.Services
{
    public interface IRatingService : IDataService
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
        Task<DataServiceResult> RecountAsync(Guid itemId, RatingRecountTypes recountType);
    }
    public class RatingService : DataService, IRatingService
    {
        private readonly AppUserManager userManager;

        public RatingService(IUnitOfWork database, ILogger logger, AppUserManager userManager) : base(database, logger)
        {
            this.userManager = userManager;
        }

        public async Task<CommentRatingModel> GetByCommentAndUserAsync(Guid commentId, string userId)
        {
            var repo = Database.GetRepo<RatingRepository, Rating>();

            var rating = await repo.GetByUserAndCommentAsync(commentId, userId);
            if (rating == default(Rating))
                return null;

            return Mapper.Map<CommentRatingModel>(rating);
        }

        public async Task<PostRatingModel> GetByPostAndUserAsync(Guid postId, string userId)
        {
            var repo = Database.GetRepo<RatingRepository, Rating>();
            var rating = await repo.GetByUserAndPostAsync(postId, userId);
            if (rating == default(Rating))
                return null;

            return Mapper.Map<PostRatingModel>(rating);
        }

        public async Task RateCommentAsync(CommentRatingModel model)
        {
            var repo = Database.GetRepo<RatingRepository, Rating>();
            var rating = Mapper.Map<Rating>(model);
            rating.RateDate = DateTime.UtcNow;

            var existingRating = await repo.GetByUserAndCommentAsync(rating.CommentId, rating.UserId);
            //Если существует, обновляем рейтинг, иначе добавляем новый
            if (existingRating != null)
            {
                await repo.UpdateAsync(existingRating, rating.Value);
            }
            else
            {
                var comment = await Database.GetRepo<CommentsRepository, Comment>().GetAsync((Guid)rating.CommentId);
                rating.OwnerId = comment.UserId;
                await repo.AddAsync(rating);
            }
        }

        public async Task RatePostAsync(PostRatingModel ratingModel)
        {
            var rating = Mapper.Map<Rating>(ratingModel);
            rating.RateDate = DateTime.UtcNow;
            var repo = Database.GetRepo<RatingRepository, Rating>();

            var existingRating = await repo.GetByUserAndPostAsync((Guid)rating.PostId, rating.UserId);
            //Если существует, обновляем рейтинг, иначе добавляем новый
            if (existingRating != null)
            {
                await repo.UpdateAsync(existingRating, rating.Value);
            }
            else
            {
                var post = await Database.GetRepo<PostsRepository, Post>().GetAsync((Guid)rating.PostId);
                rating.OwnerId = post.UserId;
                await repo.AddAsync(rating);
            }
        }

        public async Task<DataServiceResult> RecountAsync(Guid itemId, RatingRecountTypes recountType)
        {
            var repo = Database.GetRepo<RatingRepository, Rating>();
            try
            {
                switch (recountType)
                {
                    case RatingRecountTypes.AuthorService:
                        var service = await Database.GetRepo<AuthorServiceRepository, AuthorService>().GetAsync(itemId);
                        await repo.RecountAsync(service);
                        break;
                    case RatingRecountTypes.Comment:
                        var comment = await Database.GetRepo<CommentsRepository, Comment>().GetAsync(itemId);
                        await repo.RecountAsync(comment);
                        break;
                    case RatingRecountTypes.Post:
                        var post = await Database.GetRepo<PostsRepository, Post>().GetAsync(itemId);
                        await repo.RecountAsync(post);
                        break;
                    case RatingRecountTypes.User:
                        var user = await userManager.FindByIdAsync(itemId.ToString());
                        await repo.RecountAsync(user);
                        break;
                    default:
                        return DataServiceResult.Failed("Неизвестный тип для пересчета рейтинга");
                }
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при пересчете рейтинга", e);
            }
        }
    }
}