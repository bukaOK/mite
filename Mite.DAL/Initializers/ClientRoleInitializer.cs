using Mite.CodeData.Constants;
using Mite.DAL.Initializers.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Mite.DAL.Initializers
{
    internal class ClientRoleInitializer : DBInitializer
    {
        public override bool Initialized => DbContext.Roles.Any(x => x.Name == RoleNames.Client);

        private readonly RoleManager<IdentityRole> roleManager;

        public ClientRoleInitializer(RoleManager<IdentityRole> roleManager, AppDbContext dbContext) : base(dbContext)
        {
            this.roleManager = roleManager;
        }

        public override void Initialize()
        {
            roleManager.Create(new IdentityRole { Name = RoleNames.Client });
        }
    }
}
