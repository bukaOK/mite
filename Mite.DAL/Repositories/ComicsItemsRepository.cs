using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;

namespace Mite.DAL.Repositories
{
    public class ComicsItemsRepository : Repository<ComicsItem>
    {
        public ComicsItemsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
