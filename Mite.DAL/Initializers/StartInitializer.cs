using Mite.DAL.Initializers.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Microsoft.AspNet.Identity;
using Mite.DAL.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using Mite.CodeData.Enums;

namespace Mite.DAL.Initializers
{
    internal class StartInitializer : DBInitializer
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public override bool Initialized => DbContext.Roles.Any(r => r.Name == "admin") || DbContext.Users.Any(u => u.UserName == "landenor");

        public StartInitializer(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext dbContext) : base(dbContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public override void Initialize()
        {
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
