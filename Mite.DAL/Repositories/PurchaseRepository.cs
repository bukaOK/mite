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
    public class PurchaseRepository : Repository<Purchase>
    {
        public PurchaseRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Является пользователь покупателем
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<bool> IsBuyerAsync(string userId, Guid productId)
        {
            return Table.AnyAsync(x => x.BuyerId == userId && x.ProductId == productId);
        }
    }
}
