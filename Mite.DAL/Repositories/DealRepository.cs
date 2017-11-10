using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Mite.CodeData.Enums;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class DealRepository : Repository<Deal>
    {
        public DealRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<IEnumerable<Deal>> GetIncomingAsync(DealStatuses status, string authorId)
        {
            var deals = await Table.Where(x => x.AuthorId == authorId && x.Status == status)
                .Include(x => x.Service)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
            return deals;
        }
        public async Task<IEnumerable<Deal>> GetOutgoingAsync(DealStatuses status, string clientId)
        {
            var deals = await Table.Where(x => x.ClientId == clientId && x.Status == status)
                .Include(x => x.Service)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
            return deals;
        }
        public async Task<IEnumerable<Deal>> GetForModerAsync(DealStatuses status, string moderId)
        {
            var query = Table.Where(x => x.Status == status && x.ClientId != moderId && x.AuthorId != moderId);
            switch (status)
            {
                case DealStatuses.Dispute:
                    query = query.Where(x => x.ModerId == null);
                    break;
                default:
                    query = query.Where(x => x.ModerId == moderId);
                    break;
            }
            return await query.Include(x => x.Service)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
        }
        public Task<Deal> GetWithServiceAsync(long id)
        {
            return Table.Include(x => x.Service).Include(x => x.Author).Include(x => x.DisputeChat).Include(x => x.Moder)
                .Include(x => x.Client).Include(x => x.Chat.Members).FirstOrDefaultAsync(x => x.Id == id);
        }
        public int GetAuthorCounts(string authorId, DealStatuses dealType)
        {
            return Table.Count(x => x.AuthorId == authorId && x.Status == dealType);
        }
        public int GetClientCounts(string clientId)
        {
            return Table.Count(x => x.ClientId == clientId && x.Status == DealStatuses.ExpectPayment || x.Status == DealStatuses.ExpectClient);
        }
    }
}
