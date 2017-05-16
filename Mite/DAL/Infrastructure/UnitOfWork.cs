using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Mite.DAL.Repositories;

namespace Mite.DAL.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection db;

        private PostsRepository postsRepository;
        private TagsRepository tagsRepository;
        private CommentsRepository commentsRepository;
        private RatingRepository ratingRepository;
        private FollowersRepository followersRepository;
        private NotificationRepository notificationRepository;
        private HelpersRepository helpersRepository;

        public PostsRepository PostsRepository => postsRepository ?? (postsRepository = new PostsRepository(db));
        public TagsRepository TagsRepository => tagsRepository ?? (tagsRepository = new TagsRepository(db));
        public CommentsRepository CommentsRepository
            => commentsRepository ?? (commentsRepository = new CommentsRepository(db));
        public RatingRepository RatingRepository => ratingRepository ?? (ratingRepository = new RatingRepository(db));

        public FollowersRepository FollowersRepository 
            => followersRepository ?? (followersRepository = new FollowersRepository(db));

        public NotificationRepository NotificationRepository 
            => notificationRepository ?? (notificationRepository = new NotificationRepository(db));

        public HelpersRepository HelpersRepository => helpersRepository ?? (helpersRepository = new HelpersRepository(db));

        public UnitOfWork()
        {
            db = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }
    }
}