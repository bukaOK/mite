using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using System.Linq;
using System.Data.Entity;
using Mite.CodeData.Enums;

namespace Mite.DAL.Repositories
{
    public class PaymentsRepository : Repository<Payment>
    {
        public PaymentsRepository(AppDbContext db) : base(db)
        {
        }
        public async Task<IEnumerable<Payment>> GetByUserAsync(string userId)
        {
            var payments = await Table.AsNoTracking().Where(x => x.UserId == userId && x.Status == PaymentStatus.Payed)
                .OrderByDescending(x => x.Date).ToListAsync();
            return payments;
        }
        public Task<Payment> GetLastOperationAsync(PaymentType paymentType)
        {
            return Table.Where(x => x.PaymentType == paymentType).OrderByDescending(x => x.OperationId).FirstOrDefaultAsync();
        }
    }
}