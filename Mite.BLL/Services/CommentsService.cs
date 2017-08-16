using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using Mite.DAL.Infrastructure;
using System.Threading.Tasks;
using AutoMapper;
using Mite.DAL.Entities;
using System.Linq;
using Mite.DAL.Repositories;
using Mite.BLL.DTO;

namespace Mite.BLL.Services
{
    public interface ICommentsService : IDataService
    {
        Task<IEnumerable<CommentDTO>> GetCommentsByPostAsync(Guid postId, string currentUserId);
        Task<CommentDTO> AddCommentToPostAsync(CommentDTO model);
        Task UpdateCommentAsync(CommentDTO model);
        Task DeleteCommentAsync(Guid id);
        Task<string> GetCommentUserIdAsync(Guid id);
    }
    public class CommentsService : DataService, ICommentsService
    {
        public CommentsService(IUnitOfWork database) : base(database)
        {
        }

        public async Task<CommentDTO> AddCommentToPostAsync(CommentDTO dto)
        {
            var comment = Mapper.Map<Comment>(dto);
            comment.PublicTime = DateTime.UtcNow;
            comment.Rating = 0;
            comment.Id = Guid.NewGuid();

            var repo = Database.GetRepo<CommentsRepository, Comment>();
            await repo.AddAsync(comment);
            comment = await repo.GetFullAsync(comment.Id);
            return Mapper.Map<CommentDTO>(comment);
        }

        public Task DeleteCommentAsync(Guid id)
        {
            var repo = Database.GetRepo<CommentsRepository, Comment>();
            return repo.RemoveAsync(id);
        }
        public async Task<IEnumerable<CommentDTO>> GetCommentsByPostAsync(Guid postId, string currentUserId)
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

            var commentDtoList = Mapper.Map<IEnumerable<CommentDTO>>(comments);
            var currentUserRatingDtoList = Mapper.Map<IEnumerable<RatingDTO>>(currentUserRatings);

            foreach(var commentModel in commentDtoList)
            {
                commentModel.CurrentRating = currentUserRatingDtoList.FirstOrDefault(x => x.CommentId == commentModel.Id);
            }

            return commentDtoList.OrderByDescending(x => x.PublicTime);
        }

        public async Task<string> GetCommentUserIdAsync(Guid id)
        {
            var repo = Database.GetRepo<CommentsRepository, Comment>();
            var comment = await repo.GetAsync(id);
            return comment.UserId;
        }

        public Task UpdateCommentAsync(CommentDTO dto)
        {
            var repo = Database.GetRepo<CommentsRepository, Comment>();
            var comment = Mapper.Map<Comment>(dto);
            comment.PublicTime = DateTime.UtcNow;

            return repo.UpdateAsync(comment);
        }
    }
}