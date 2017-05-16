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
    }
}