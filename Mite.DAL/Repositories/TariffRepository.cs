using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System;

namespace Mite.DAL.Repositories
{
    public class TariffRepository : Repository<AuthorTariff>
    {
        public TariffRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<IEnumerable<AuthorTariff>> GetByAuthorAsync(string authorId)
        {
            return await Table.Where(x => x.AuthorId == authorId).ToListAsync();
        }
        public async Task<IEnumerable<AuthorTariff>> GetByClientAsync(string clientId)
        {
            return await DbContext.ClientTariffs.Include(x => x.Tariff.Author)
                .Where(x => x.ClientId == clientId).Select(x => x.Tariff).ToListAsync();
        }
        public async Task<bool> HasAnyClientsAsync(Guid tariffId)
        {
            return await DbContext.ClientTariffs.AnyAsync(x => x.TariffId == tariffId);
        }
        
        public IEnumerable<ClientTariff> NotPaidTariffs()
        {
            return DbContext.ClientTariffs.Include(x => x.Tariff).Where(x => x.PayStatus == CodeData.Enums.TariffStatuses.Paid).ToList();
        }
    }
}
