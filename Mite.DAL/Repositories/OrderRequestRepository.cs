using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class OrderRequestRepository : Repository<OrderRequest>
    {
        public OrderRequestRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<OrderRequest> GetAsync(string executerId, Guid orderId)
        {
            return await Table.FirstOrDefaultAsync(x => x.ExecuterId == executerId && x.OrderId == orderId);
        }
        public async Task<OrderRequest> GetAsync(Guid orderId, string executerId)
        {
            return await Table.FirstOrDefaultAsync(x => x.ExecuterId == executerId && x.OrderId == orderId);
        }
        public async Task<IEnumerable<OrderRequest>> GetByOrderAsync(Guid orderId)
        {
            return await Table.AsNoTracking().Include(x => x.Executer).Where(x => x.OrderId == orderId).ToListAsync();
        }
    }
}
