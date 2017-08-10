﻿using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using System.Linq;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class PaymentsRepository : Repository<Payment>
    {
        public PaymentsRepository(AppDbContext db) : base(db)
        {
        }
        public async Task<IEnumerable<Payment>> GetByUserAsync(string userId)
        {
            var payments = await Table.Where(x => x.UserId == userId).OrderByDescending(x => x.Date).ToListAsync();
            return payments;
        }
    }
}