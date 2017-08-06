using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class SocialServicesRepository : Repository<SocialService>
    {
        public SocialServicesRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public IEnumerable<SocialService> GetByUser(string userId)
        {
            var query = $"select * from dbo.UserSocialServices inner join dbo.{TableName} on "
                + $"dbo.UserSocialServices.SocialServiceName=dbo.{TableName}.Name where dbo.UserSocialServices.UserId=@userId";
            return Db.Query<SocialService>(query, new { userId });
        }
        public Task<IEnumerable<SocialService>> GetByUserAsync(string userId)
        {
            var query = $"select * from dbo.UserSocialServices inner join dbo.{TableName} on "
                + $"dbo.UserSocialServices.SocialServiceName=dbo.{TableName}.Name where dbo.UserSocialServices.UserId=@userId";
            return Db.QueryAsync<SocialService>(query, new { userId });
        }
    }
}