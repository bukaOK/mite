namespace Mite.DAL.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Mite.CodeData.Enums;
    using Mite.DAL.Entities;
    using Mite.DAL.Initializers;
    using Mite.DAL.Initializers.Core;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<Mite.DAL.Infrastructure.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Mite.DAL.Infrastructure.AppDbContext context)
        {
            base.Seed(context);

            var userManager = new UserManager<User>(new UserStore<User>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var initializers = new List<IInitializer>
            {
                new StartInitializer(userManager, roleManager, context),
                new ClientRoleInitializer(roleManager, context)
            };
            foreach(var init in initializers)
            {
                if (!init.Initialized)
                    init.Initialize();
            }
        }
    }
}
