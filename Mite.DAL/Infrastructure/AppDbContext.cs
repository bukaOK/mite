using Microsoft.AspNet.Identity.EntityFramework;
using Mite.DAL.Entities;
using System.Data.Entity;

namespace Mite.DAL.Infrastructure
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public IDbSet<Post> Posts { get; set; }
        public IDbSet<Group> Groups { get; set; }
        public IDbSet<Comment> Comments { get; set; }
        public IDbSet<Notification> Notifications { get; set; }
        public IDbSet<Helper> Helpers { get; set; }
        public IDbSet<Payment> Payments { get; set; }
        public IDbSet<Follower> Followers { get; set; }
        public IDbSet<Tag> Tags { get; set; }
        public IDbSet<CashOperation> CashOperations { get; set; }
        public IDbSet<ExternalService> ExternalServices { get; set; }
        public IDbSet<SocialLinks> SocialLinks { get; set; }
        public IDbSet<City> Cities { get; set; }
        public IDbSet<AuthorService> AuthorServices { get; set; }
        public IDbSet<AuthorServiceType> AuthorServiceTypes { get; set; }
        public IDbSet<Deal> Deals { get; set; }

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
        }
    }
}