using Dapper;
using Mite.CodeData.Enums;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class ClientTariffRepository : Repository<ClientTariff>
    {
        public ClientTariffRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public Task<ClientTariff> GetAsync(Guid tariffId, string clientId)
        {
            return Table.FirstOrDefaultAsync(x => x.TariffId == tariffId && x.ClientId == clientId);
        }
        /// <summary>
        /// Ищем, подписан ли клиент на какой либо тариф автора
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        public Task<ClientTariff> GetByClientFirstAsync(string clientId, string authorId)
        {
            return Table.Include(x => x.Tariff).FirstOrDefaultAsync(x => x.ClientId == clientId && x.Tariff.AuthorId == authorId);
        }
        /// <summary>
        /// Получить платные подписки клиента
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ClientTariff>> GetByClientAsync(string clientId)
        {
            return await Table.AsNoTracking().Include(x => x.Tariff.Author).Where(x => x.ClientId == clientId).ToListAsync();
        }
        public async Task RemoveAuthorTariffsAsync(string clientId, string authorId)
        {
            var tariffs = await Table.Where(x => x.ClientId == clientId && x.Tariff.AuthorId == authorId).ToListAsync();
            if(tariffs != null && tariffs.Count > 0)
            {
                Table.RemoveRange(tariffs);
                await SaveAsync();
            }
        }

        public Task<bool> IsExistAsync(Guid tariffId, string clientId)
        {
            return Table.AnyAsync(x => x.TariffId == tariffId & x.ClientId == clientId);
        }

        public async Task<int> GetSponsorsCountAsync(string authorId)
        {
            return await Table.CountAsync(x => x.Tariff.AuthorId == authorId);
        }

        public Task<IEnumerable<User>> GetSponsorsAsync(string userId, SortFilter sort)
        {
            var query = "select * from (select distinct on(clients.\"Id\") clients.*, \"LastPayTimeUtc\" from dbo.\"AuthorTariffs\" tariffs left outer join " +
                "dbo.\"ClientTariffs\" client_tariffs on client_tariffs.\"TariffId\"=tariffs.\"Id\" left outer join dbo.\"Users\" clients " +
                "on clients.\"Id\"=client_tariffs.\"ClientId\" where tariffs.\"AuthorId\"=@userId and client_tariffs is not null) as tbl order by ";
            switch (sort)
            {
                case SortFilter.New:
                    query += "tbl.\"LastPayTimeUtc\" desc;";
                    break;
                case SortFilter.Old:
                    query += "tbl.\"LastPayTimeUtc\" asc;";
                    break;
                default:
                    query += "tbl.\"Rating\" desc;";
                    break;
            }
            return Db.QueryAsync<User>(query, new { userId });
        }
    }
}
