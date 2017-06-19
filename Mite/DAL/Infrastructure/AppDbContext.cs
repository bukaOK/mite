using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using Mite.DAL.Entities;
using System.Data.Entity;

namespace Mite.DAL.Infrastructure
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Helper> Helpers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CashOperation> CashOperations { get; set; }
        public DbSet<ExternalService> ExternalServices { get; set; }
        public DbSet<SocialLinks> SocialLinks { get; set; }

        public AppDbContext()
            : base("DefaultConnection")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<User>().ToTable("Users");
            //modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            //modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            //modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            //modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
        }
    }
}