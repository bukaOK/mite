﻿using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using Mite.DAL.Infrastructure;
using System.Threading.Tasks;
using Mite.Models;
using AutoMapper;
using Mite.DAL.Entities;
using System.Linq;
using Mite.Enums;
using Mite.DAL.Repositories;

namespace Mite.BLL.Services
{
    public interface ICommentsService : IDataService
    {
        Task<IEnumerable<CommentModel>> GetCommentsByPostAsync(Guid postId, string currentUserId);
        Task<CommentModel> AddCommentToPostAsync(CommentModel model);
        Task UpdateCommentAsync(CommentModel model);
        Task DeleteCommentAsync(Guid id);
        Task<string> GetCommentUserIdAsync(Guid id);
    }
    public class CommentsService : DataService, ICommentsService
    {
        public CommentsService(IUnitOfWork database) : base(database)
        {
        }

        public async Task<CommentModel> AddCommentToPostAsync(CommentModel model)
        {
            var comment = Mapper.Map<Comment>(model);
            comment.PublicTime = DateTime.UtcNow;
            comment.Rating = 0;
            comment.Id = Guid.NewGuid();

            var repo = Database.GetRepo<CommentsRepository, Comment>();
            await repo.AddAsync(comment);
            comment = await repo.GetFullAsync(comment.Id);
            return Mapper.Map<CommentModel>(comment);
        }

        public Task DeleteCommentAsync(Guid id)
        {
            var repo = Database.GetRepo<CommentsRepository, Comment>();
            return repo.RemoveAsync(id);
        }
        public async Task<IEnumerable<CommentModel>> GetCommentsByPostAsync(Guid postId, string currentUserId)
        {
            var repo = Database.GetRepo<CommentsRepository, Comment>();
            var ratingRepo = Database.GetRepo<RatingRepository, Rating>();
            //Получаем комментарии
            var comments = await repo.GetListByPostAsync(postId);
            //Получаем рейтинги текущего пользователя
            var currentUserRatings = await ratingRepo.GetCommentsRatings(comments.Select(x => x.Id), currentUserId);

            foreach(var comment in comments)
            {
                comment.ParentComment = comments.FirstOrDefault(x => comment.ParentCommentId == x.Id);
            }

            var commentModels = Mapper.Map<IEnumerable<CommentModel>>(comments);
            var currentUserRatingModels = Mapper.Map<IEnumerable<CommentRatingModel>>(currentUserRatings);

            foreach(var commentModel in commentModels)
            {
                commentModel.CurrentRating = currentUserRatingModels.FirstOrDefault(x => x.CommentId == commentModel.Id);
            }

            return commentModels.OrderByDescending(x => x.PublicTime);
        }

        public async Task<string> GetCommentUserIdAsync(Guid id)
        {
            var repo = Database.GetRepo<CommentsRepository, Comment>();
            var comment = await repo.GetAsync(id);
            return comment.UserId;
        }

        public Task UpdateCommentAsync(CommentModel model)
        {
            var repo = Database.GetRepo<CommentsRepository, Comment>();
            var comment = Mapper.Map<Comment>(model);
            comment.PublicTime = DateTime.UtcNow;

            return repo.UpdateAsync(comment);
        }
    }
}