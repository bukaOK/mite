using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class AnonymousPurchaseRepository : Repository<AnonymousPurchase>
    {
        public AnonymousPurchaseRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public Task<AnonymousPurchase> GetAsync(string email, string code)
        {
            return Table.FirstOrDefaultAsync(x => x.UserEmail == email && x.Code == code);
        }
    }
}
