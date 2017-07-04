using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Mite.DAL.Repositories;
using Mite.DAL.Core;
using System.Collections.Generic;
using System.Linq;

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
        private PaymentsRepository paymentsRepository;
        private CashOperationsRepository cashOperationsRepository;
        private ExternalServiceRepository externalServiceRepository;
        private SocialLinksRepository socialLinksRepository;
        private UserRepository userRepository;

        public PostsRepository PostsRepository => postsRepository ?? (postsRepository = new PostsRepository(db));
        public TagsRepository TagsRepository => tagsRepository ?? (tagsRepository = new TagsRepository(db));
        public CommentsRepository CommentsRepository =>
            commentsRepository ?? (commentsRepository = new CommentsRepository(db));
        public RatingRepository RatingRepository => ratingRepository ?? (ratingRepository = new RatingRepository(db));

        public FollowersRepository FollowersRepository =>
            followersRepository ?? (followersRepository = new FollowersRepository(db));

        public NotificationRepository NotificationRepository 
            => notificationRepository ?? (notificationRepository = new NotificationRepository(db));

        public HelpersRepository HelpersRepository => helpersRepository ?? (helpersRepository = new HelpersRepository(db));

        public PaymentsRepository PaymentsRepository => paymentsRepository ?? (paymentsRepository = new PaymentsRepository(db));
        public CashOperationsRepository CashOperationsRepository => 
            cashOperationsRepository ?? (cashOperationsRepository = new CashOperationsRepository(db));

        public ExternalServiceRepository ExternalServiceRepository => 
            externalServiceRepository ?? (externalServiceRepository = new ExternalServiceRepository(db));

        public SocialLinksRepository SocialLinksRepository =>
            socialLinksRepository ?? (socialLinksRepository = new SocialLinksRepository(db));
        public UserRepository UserRepository => userRepository ?? (userRepository = new UserRepository(db));


        public UnitOfWork()
        {
            db = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }
        public UnitOfWork(IDbConnection dbConnection)
        {
            db = dbConnection;
        }
    }
}