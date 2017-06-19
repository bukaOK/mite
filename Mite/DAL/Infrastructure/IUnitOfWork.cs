using Mite.DAL.Core;
using Mite.DAL.Repositories;

namespace Mite.DAL.Infrastructure
{
    public interface IUnitOfWork
    {
        PostsRepository PostsRepository { get; }
        TagsRepository TagsRepository { get; }
        CommentsRepository CommentsRepository { get; }
        RatingRepository RatingRepository { get; }
        FollowersRepository FollowersRepository { get; }
        NotificationRepository NotificationRepository { get; }
        HelpersRepository HelpersRepository { get; }
        PaymentsRepository PaymentsRepository { get; }
        CashOperationsRepository CashOperationsRepository { get; }
        ExternalServiceRepository ExternalServiceRepository { get; }
        SocialLinksRepository SocialLinksRepository { get; }

        TRepo GetRepository<TRepo, TEntity>() where TRepo : class, IRepository<TEntity> where TEntity : class, IEntity;
    }
}