using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.DataProtection;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.CodeData.Enums;
using System;
using System.Data.Entity;

namespace Mite
{
    public class DbInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        IDataProtectionProvider _dataProtectionProvider;

        public DbInitializer(IDataProtectionProvider dataProtectionProvider) : base()
        {
            _dataProtectionProvider = dataProtectionProvider;
        }
        protected override void Seed(AppDbContext context)
        {
            var userManager = new AppUserManager(new UserStore<User>(context), _dataProtectionProvider);
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());

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
            base.Seed(context);
        }
    }
}