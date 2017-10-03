using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class AuthorServiceRepository : Repository<AuthorService>
    {
        public AuthorServiceRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public Task<AuthorService> GetWithServiceTypeAsync(Guid id)
        {
            return Table.Include(x => x.ServiceType).Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<AuthorService>> GetByUserAsync(string userId)
        {
            var services = await Table.Where(x => x.AuthorId == userId).Include(x => x.ServiceType).ToListAsync();
            return services;
        }
    }
}
