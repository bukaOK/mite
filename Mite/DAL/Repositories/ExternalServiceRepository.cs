using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Dapper;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class ExternalServiceRepository : Repository<ExternalService>
    {
        public ExternalServiceRepository(IDbConnection db) : base(db)
        {
        }
        public Task<ExternalService> GetAsync(string userId, string serviceName)
        {
            var query = "select * from dbo.ExternalServices where UserId=@userId and Name=@serviceName";
            return Db.QueryFirstOrDefaultAsync<ExternalService>(query, new { userId, serviceName });
        }
        public async Task<ExternalService> GetByProviderAsync(string providerKey, string serviceName)
        {
            var query = "select * from dbo.AspNetUserLogins where LoginProvider=@serviceName and ProviderKey=@providerKey";
            var login = await Db.QueryFirstOrDefaultAsync<dynamic>(query, new { serviceName, providerKey });
            var userId = login.UserId;

            return await GetAsync(userId, serviceName);
        }
        public async Task RemoveAsync(string userId, string serviceName)
        {
            var query = "delete from dbo.ExternalServices where UserId=@userId and ServiceName=@serviceName";
            await Db.ExecuteAsync(query, new { userId, serviceName });
        }
    }
}