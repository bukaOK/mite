﻿using Microsoft.AspNet.Identity.EntityFramework;
using Mite.DAL.Entities;
using System.Data.Entity;

namespace Mite.DAL.Infrastructure
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<PostCollectionItem> PostCollectionItems { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Helper> Helpers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Follower> Followers { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CashOperation> CashOperations { get; set; }
        public DbSet<ExternalService> ExternalServices { get; set; }
        public DbSet<SocialLinks> SocialLinks { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<AuthorService> AuthorServices { get; set; }
        public DbSet<AuthorServiceType> AuthorServiceTypes { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatMessageUser> MessageUsers { get; set; }
        public DbSet<FavoritePost> FavoritePosts { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }

        public AppDbContext()
            : base("DefaultConnection")
        {
        }
        public AppDbContext(string connection) : base(connection) {}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");

            modelBuilder.Entity<Chat>().HasMany(x => x.Users).WithMany()
                .Map(x =>
                {
                    x.ToTable("ChatUsers");
                    x.MapLeftKey("ChatId");
                    x.MapRightKey("UserId");
                });

        }
    }
}