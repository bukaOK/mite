using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Linq;
using Dapper;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class ExternalServiceRepository : Repository<ExternalService>
    {
        public ExternalServiceRepository(AppDbContext db) : base(db)
        {
        }
        public Task<ExternalService> GetAsync(string userId, string serviceName)
        {
            return Table.FirstOrDefaultAsync(x => x.UserId == userId && x.Name == serviceName);
        }
        public async Task<ExternalService> GetByProviderAsync(string providerKey, string serviceName)
        {
            var query = "select * from dbo.\"UserLogins\" where \"LoginProvider\"=@serviceName and \"ProviderKey\"=@providerKey;";
            var login = await Db.QueryFirstOrDefaultAsync<dynamic>(query, new { serviceName, providerKey });
            var userId = login.UserId;

            return await GetAsync(userId, serviceName);
        }
        /// <summary>
        /// Получить сервис с токеном по имени внешнего сервиса
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public Task<ExternalService> GetByServiceNameAsync(string serviceName)
        {
            return Table.FirstOrDefaultAsync(x => x.Name == serviceName);
        }
        public void Remove(string userId, string serviceName)
        {
            var existingService = Table.FirstOrDefault(x => x.UserId == userId && x.Name == serviceName);
            Table.Remove(existingService);
            Save();
        }
        public async Task RemoveAsync(string userId, string serviceName)
        {
            var existingService = await Table.FirstOrDefaultAsync(x => x.UserId == userId && x.Name == serviceName);
            Table.Remove(existingService);
            await SaveAsync();
        }
    }
}