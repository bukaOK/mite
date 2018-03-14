using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System;
using System.Threading.Tasks;

namespace Mite.BLL.Services
{
    public interface IUserReviewService : IDataService
    {
        bool IsReviewLeft(string userId);
        Task<DataServiceResult> AddAsync(string userId, string review);
    }
    public class UserReviewService : DataService, IUserReviewService
    {
        private readonly ReviewsRepository repo;

        public UserReviewService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
            repo = Database.GetRepo<ReviewsRepository, UserReview>();
        }

        public async Task<DataServiceResult> AddAsync(string userId, string review)
        {
            try
            {
                await repo.AddAsync(new UserReview
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Review = review
                });
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при добавлении отзыва пользователя", e);
            }
        }

        public bool IsReviewLeft(string userId)
        {
            return repo.GetByUser(userId) != null;
        }
    }
}