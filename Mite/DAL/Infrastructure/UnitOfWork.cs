using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Mite.DAL.Repositories;

namespace Mite.DAL.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _db;

        private PostsRepository _postsRepository;
        private TagsRepository _tagsRepository;
        private CommentsRepository _commentsRepository;
        private RatingRepository _ratingRepository;
        private FollowersRepository _followersRepository;
        private NotificationRepository _notificationRepository;

        public PostsRepository PostsRepository => _postsRepository ?? (_postsRepository = new PostsRepository(_db));
        public TagsRepository TagsRepository => _tagsRepository ?? (_tagsRepository = new TagsRepository(_db));
        public CommentsRepository CommentsRepository
            => _commentsRepository ?? (_commentsRepository = new CommentsRepository(_db));
        public RatingRepository RatingRepository => _ratingRepository ?? (_ratingRepository = new RatingRepository(_db));

        public FollowersRepository FollowersRepository 
            => _followersRepository ?? (_followersRepository = new FollowersRepository(_db));

        public NotificationRepository NotificationRepository 
            => _notificationRepository ?? (_notificationRepository = new NotificationRepository(_db));

        public UnitOfWork()
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }
    }
}