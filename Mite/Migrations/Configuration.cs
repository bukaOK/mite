using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Mite.Enums;

namespace Mite.Migrations
{    
    internal sealed class Configuration : DbMigrationsConfiguration<AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(AppDbContext context)
        {
            base.Seed(context);

            if (!context.Roles.Any(r => r.Name == "admin") || !context.Users.Any(u => u.UserName == "landenor"))
            {
                var userManager = new UserManager<User>(new UserStore<User>(context));
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                var adminRole = new IdentityRole { Name = "admin" };
                var moderRole = new IdentityRole { Name = "moder" };
                var userRole = new IdentityRole { Name = "user" };

                roleManager.Create(adminRole);
                roleManager.Create(moderRole);
                roleManager.Create(userRole);

                var admin = new User
                {
                    Email = "ponchitos16@gmail.com",
                    UserName = "landenor",
                    Gender = (byte)Genders.Male,
                    Age = 19,
                    RegisterDate = DateTime.UtcNow,
                    AvatarSrc = "/Content/images/male.png"
                };
                const string pass = "turbo1631";
                var result = userManager.Create(admin, pass);

                //если создание пользователя успешно
                if (result.Succeeded)
                {
                    userManager.AddToRoles(admin.Id, adminRole.Name, moderRole.Name, userRole.Name);
                }
            }
        }
    }
}
