using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Mite.DAL.Infrastructure;

namespace Mite.BLL.IdentityManagers
{
    public class AppRoleManager : RoleManager<IdentityRole>
    {
        public AppRoleManager(AppDbContext dbContext) : base(new RoleStore<IdentityRole>(dbContext)) { }
    }
}