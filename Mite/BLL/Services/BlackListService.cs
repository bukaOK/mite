using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.BLL.Services
{
    public interface IBlackListService : IDataService
    {
        /// <summary>
        /// Может ли один другого оценивать
        /// </summary>
        /// <param name="model"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        Task<bool> CanRatePostAsync(PostRatingModel model);
        Task<bool> CanRateCommentAsync(CommentRatingModel model);
        Task<bool> CanCommentAsync(CommentModel model);
    }
    public class BlackListService : DataService, IBlackListService
    {
        private readonly BlackListUserRepository blackListRepo;

        public BlackListService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
            blackListRepo = Database.GetRepo<BlackListUserRepository, BlackListUser>();
        }

        public async Task<bool> CanCommentAsync(CommentModel model)
        {
            var post = await Database.GetRepo<PostsRepository, Post>().GetAsync(model.PostId);
            var isInBlackList = await HasActionAccessAsync(post.UserId, model.User.Id);
            return !isInBlackList;
        }

        public async Task<bool> CanRateCommentAsync(CommentRatingModel model)
        {
            var comment = await Database.GetRepo<CommentsRepository, Comment>().GetAsync(model.CommentId);
            var isInBlackList = await HasActionAccessAsync(comment.UserId, model.UserId);
            return !isInBlackList;
        }

        public async Task<bool> CanRatePostAsync(PostRatingModel model)
        {
            var post = await Database.GetRepo<PostsRepository, Post>().GetAsync(model.PostId);
            var isInBlackList = await HasActionAccessAsync(post.UserId, model.UserId);
            return !isInBlackList;
        }

        private async Task<bool> HasActionAccessAsync(string firstUserId, string secondUserId)
        {
            return (await blackListRepo.IsInBlackList(firstUserId, secondUserId)) ||
                        (await blackListRepo.IsInBlackList(secondUserId, firstUserId));
        }
    }
}