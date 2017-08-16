using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Infrastructure;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class HelpersRepository : Repository<Helper>
    {
        public HelpersRepository(AppDbContext db) : base(db)
        {
        }
        public async Task<Helper> GetByUserAsync(string userId)
        {
            var helper = await Table.FirstOrDefaultAsync(x => x.UserId == userId);
            return helper;
        }
        public Helper GetByUser(string userId)
        {
            return Table.FirstOrDefault(x => x.UserId == userId);
        }
    }
}