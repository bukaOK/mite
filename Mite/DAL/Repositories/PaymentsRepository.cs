using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Infrastructure;

namespace Mite.DAL.Repositories
{
    public class PaymentsRepository : Repository<Payment>
    {
        public PaymentsRepository(AppDbContext db) : base(db)
        {
        }
        public Task<IEnumerable<Payment>> GetByUserAsync(string userId)
        {
            var query = "select * from dbo.Payments where UserId=@userId order by Date desc";
            return Db.QueryAsync<Payment>(query, new { userId });
        }
    }
}