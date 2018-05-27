using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;

namespace Mite.DAL.Repositories
{
    public class CollectionItemsRepository : Repository<PostCollectionItem>
    {
        public CollectionItemsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
